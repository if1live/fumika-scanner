package main

import (
	"context"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"strconv"
	"time"

	"github.com/if1live/fumika"

	"net/http"

	"golang.org/x/oauth2/google"
	spreadsheet "gopkg.in/Iwark/spreadsheet.v2"
)

const (
	fieldISBN      = 0
	fieldTitle     = 1
	fieldAuthor    = 2
	fieldPublisher = 3
	fieldPrice     = 4

	fieldUsedBookCmd       = 5
	fieldAladinPriceBest   = fieldUsedBookCmd + 1
	fieldAladinPriceGood   = fieldUsedBookCmd + 2
	fieldAladinPriceNormal = fieldUsedBookCmd + 3
	fieldYes24PriceBest    = fieldUsedBookCmd + 4
	fieldYes24PriceGood    = fieldUsedBookCmd + 5
	fieldYes24PriceNormal  = fieldUsedBookCmd + 6
	fieldUsedBookUpdatedAt = fieldUsedBookCmd + 7

	commandSkip = "skip"
)

type UsedBookFieldList struct {
	priceBest   int
	priceGood   int
	priceNormal int
}

type Config struct {
	NaverClientID     string `json:"naver_client_id"`
	NaverClientSecret string `json:"naver_client_secret"`
	SpreadsheetID     string `json:"spreadsheet_id"`
	SheetID           uint   `json:"sheet_id"`
}

func checkError(err error) {
	if err != nil {
		panic(err.Error())
	}
}

func NewConfig(filepath string) Config {
	data, err := ioutil.ReadFile(filepath)
	checkError(err)

	var config Config
	err = json.Unmarshal(data, &config)
	return config
}

func updateInfo(sheet *spreadsheet.Sheet, config Config) {
	searchAPI := NewSearchAPI(config.NaverClientID, config.NaverClientSecret)

	for rowIdx, row := range sheet.Rows {
		isbn := row[fieldISBN].Value
		title := row[fieldTitle].Value
		if title != "" {
			continue
		}

		result := searchAPI.SearchByISBN(isbn)
		item := result.FirstItem()
		if item == nil {
			fmt.Printf("cannot find ISBN : %s\n", isbn)
			continue
		}

		sheet.Update(rowIdx, fieldTitle, item.Title)
		sheet.Update(rowIdx, fieldAuthor, item.Author)
		sheet.Update(rowIdx, fieldPublisher, item.Publisher)
		sheet.Update(rowIdx, fieldPrice, item.PriceStr)
		fmt.Printf("[update info] isbn=%s -> title=%s\n", isbn, item.Title)
	}
}

func updateUsedBookRow(isbn string, sheet *spreadsheet.Sheet, rowIdx int, api fumika.SearchAPI, fields UsedBookFieldList, tag string) {
	result, err := api.SearchISBN(isbn)
	if err != nil {
		return
	}

	sheet.Update(rowIdx, fields.priceBest, strconv.Itoa(result.PriceBest))
	sheet.Update(rowIdx, fields.priceGood, strconv.Itoa(result.PriceGood))
	sheet.Update(rowIdx, fields.priceNormal, strconv.Itoa(result.PriceNormal))
	fmt.Printf("[update usedbook %s] isbn=%s -> title=%s\n", tag, isbn, result.Title)
}

func updateUsedBookPrice(sheet *spreadsheet.Sheet, config Config) {
	httpclient := &http.Client{}

	aladinAPI := fumika.NewAladin(httpclient)
	aladinFields := UsedBookFieldList{
		priceBest:   fieldAladinPriceBest,
		priceGood:   fieldAladinPriceGood,
		priceNormal: fieldAladinPriceNormal,
	}

	yes24API := fumika.NewYes24(httpclient)
	yes24Fields := UsedBookFieldList{
		priceBest:   fieldYes24PriceBest,
		priceGood:   fieldYes24PriceGood,
		priceNormal: fieldYes24PriceNormal,
	}

	now := time.Now()
	nowStr := now.Format("2006-01-02")

	for rowIdx, row := range sheet.Rows {
		cmd := row[fieldUsedBookCmd].Value
		if cmd == commandSkip {
			continue
		}

		isbn := row[fieldISBN].Value
		updateUsedBookRow(isbn, sheet, rowIdx, aladinAPI, aladinFields, "aladin")
		updateUsedBookRow(isbn, sheet, rowIdx, yes24API, yes24Fields, "yes24")

		sheet.Update(rowIdx, fieldUsedBookCmd, commandSkip)
		sheet.Update(rowIdx, fieldUsedBookUpdatedAt, nowStr)
	}
}

func main() {
	config := NewConfig("config.json")

	data, err := ioutil.ReadFile("client_secret.json")
	checkError(err)
	conf, err := google.JWTConfigFromJSON(data, spreadsheet.Scope)
	checkError(err)
	client := conf.Client(context.TODO())

	service := spreadsheet.NewServiceWithClient(client)
	spreadsheet, err := service.FetchSpreadsheet(config.SpreadsheetID)
	checkError(err)

	// get a sheet by the index.
	sheet, err := spreadsheet.SheetByID(config.SheetID)
	checkError(err)

	updateInfo(sheet, config)
	// Make sure call Synchronize to reflect the changes
	err = sheet.Synchronize()
	checkError(err)

	updateUsedBookPrice(sheet, config)
	err = sheet.Synchronize()
	checkError(err)
}
