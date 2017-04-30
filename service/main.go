package main

import (
	"context"
	"encoding/json"
	"fmt"
	"io/ioutil"

	"golang.org/x/oauth2/google"
	spreadsheet "gopkg.in/Iwark/spreadsheet.v2"
)

const (
	fieldISBN      = 0
	fieldTitle     = 1
	fieldAuthor    = 2
	fieldPublisher = 3
)

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

func main() {
	config := NewConfig("config.json")
	searchAPI := NewSearchAPI(config.NaverClientID, config.NaverClientSecret)

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
		fmt.Printf("isbn=%s -> title=%s\n", isbn, item.Title)
	}

	// Make sure call Synchronize to reflect the changes
	err = sheet.Synchronize()
	checkError(err)

}
