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

	fieldAladinCmd         = 5
	fieldAladinPriceBest   = fieldAladinCmd + 1
	fieldAladinPriceGood   = fieldAladinCmd + 2
	fieldAladinPriceNormal = fieldAladinCmd + 3
	fieldAladinUpdatedAt   = fieldAladinCmd + 4
	fieldAladinEnd         = fieldAladinCmd + 5

	fieldYes24Cmd         = fieldAladinEnd
	fieldYes24PriceBest   = fieldYes24Cmd + 1
	fieldYes24PriceGood   = fieldYes24Cmd + 2
	fieldYes24PriceNormal = fieldYes24Cmd + 3
	fieldYes24UpdatedAt   = fieldYes24Cmd + 4
	fieldYes24End         = fieldYes24Cmd + 5

	commandSkip = "skip"
)

type UsedBookFieldList struct {
	cmd         int
	priceBest   int
	priceGood   int
	priceNormal int
	updatedAt   int
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

func updateUsedBook(sheet *spreadsheet.Sheet, api fumika.SearchAPI, fields UsedBookFieldList, tag string) {
	now := time.Now()
	nowStr := now.Format("2006-01-02 15:04:05")

	for rowIdx, row := range sheet.Rows {
		cmd := row[fields.cmd].Value
		if cmd == commandSkip {
			continue
		}

		isbn := row[fieldISBN].Value
		result, err := api.SearchISBN(isbn)
		if err != nil {
			continue
		}

		sheet.Update(rowIdx, fields.cmd, commandSkip)
		sheet.Update(rowIdx, fields.priceBest, strconv.Itoa(result.PriceBest))
		sheet.Update(rowIdx, fields.priceGood, strconv.Itoa(result.PriceGood))
		sheet.Update(rowIdx, fields.priceNormal, strconv.Itoa(result.PriceNormal))
		sheet.Update(rowIdx, fields.updatedAt, nowStr)
		fmt.Printf("[update usedbook %s] isbn=%s -> title=%s\n", tag, isbn, result.Title)
	}
}

func updateYes24Data(sheet *spreadsheet.Sheet, config Config) {
	httpclient := &http.Client{}
	api := fumika.NewYes24(httpclient)
	fields := UsedBookFieldList{
		cmd:         fieldYes24Cmd,
		priceBest:   fieldYes24PriceBest,
		priceGood:   fieldYes24PriceGood,
		priceNormal: fieldYes24PriceNormal,
		updatedAt:   fieldYes24UpdatedAt,
	}
	updateUsedBook(sheet, api, fields, "yes24")
}

func updateAladinData(sheet *spreadsheet.Sheet, config Config) {
	httpclient := &http.Client{}
	api := fumika.NewAladin(httpclient)

	fields := UsedBookFieldList{
		cmd:         fieldAladinCmd,
		priceBest:   fieldAladinPriceBest,
		priceGood:   fieldAladinPriceGood,
		priceNormal: fieldAladinPriceNormal,
		updatedAt:   fieldAladinUpdatedAt,
	}
	updateUsedBook(sheet, api, fields, "aladin")
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

	updateAladinData(sheet, config)
	err = sheet.Synchronize()
	checkError(err)

	updateYes24Data(sheet, config)
	err = sheet.Synchronize()
	checkError(err)
}
