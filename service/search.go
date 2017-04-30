package main

import (
	"bytes"
	"encoding/json"
	"io"
	"net/http"
	"strconv"
)

/*
https://developers.naver.com/docs/search/book/
*/

const (
	searchURI = "https://openapi.naver.com/v1/search/book_adv.json"
)

type SearchAPI struct {
	clientID     string
	clientSecret string
}

type SearchResult struct {
	LastBuildDate string     `json:"lastBuildDate"`
	Total         int        `json:"total"`
	Start         int        `json:"start"`
	Display       int        `json:"display"`
	Items         []BookItem `json:"items"`
}

func (r *SearchResult) FirstItem() *BookItem {
	if r.Items == nil || len(r.Items) == 0 {
		return nil
	}
	return &r.Items[0]
}

type BookItem struct {
	Title       string `json:"title"`
	Link        string `json:"link"`
	Image       string `json:"image"`
	Author      string `json:"author"`
	PriceStr    string `json:"price"`
	DiscountStr string `json:"discount"`
	Publisher   string `json:"publisher"`
	PubdateStr  string `json:"pubdate"`
	ISBNStr     string `json:"isbn"`
	Description string `json:"description"`
}

func (item *BookItem) Price() int {
	val, err := strconv.Atoi(item.PriceStr)
	if err != nil {
		panic(err)
	}
	return val
}

func NewSearchAPI(clientID, clientSecret string) *SearchAPI {
	return &SearchAPI{
		clientID:     clientID,
		clientSecret: clientSecret,
	}
}

func (api *SearchAPI) SearchByISBN(isbn string) SearchResult {
	qs := "d_isbn=" + isbn

	client := &http.Client{}

	fulluri := searchURI + "?" + qs
	req, err := http.NewRequest("GET", fulluri, nil)
	checkError(err)

	req.Header.Add("X-Naver-Client-Id", api.clientID)
	req.Header.Add("X-Naver-Client-Secret", api.clientSecret)

	resp, err := client.Do(req)
	var buf bytes.Buffer
	_, err = io.Copy(&buf, resp.Body)
	checkError(err)

	err = resp.Body.Close()
	checkError(err)

	var result SearchResult
	err = json.Unmarshal(buf.Bytes(), &result)
	checkError(err)

	return result
}
