# fumika-scanner

중고책 처분 프로세스

![screenshot](https://raw.githubusercontent.com/if1live/fumika-scanner/master/document/final-result.jpeg)

## 특징
* 안드로이드 앱으로 ISBN을 스캔할 수 있다
* 중고 매각가를 조회할 수 있다
  * 알라딘, Yes24 지원

## 시스템 구성
* 데이터베이스 : 구글 스프레드시트
* 바코드 스캐너 : 안드로이드 앱, ISBN 바코드를 찍어서 구글 스프레드시트에 저장
* 데이터 갱신자 : ISBN 목록을 이용해서 도서 정보, 중고 매각가를 조회

## 사용해보기

### 준비물 - 데이터베이스
데이터베이스로 사용할 구글 스프레드시트를 만든다. 적어도 2개의 시트를 사용하는 것을 권장한다.
첫번째 시트는 바코드 스캐너로 읽은 ISBN을 저장하는 용도로 쓰인다.
두번째 시트는 ISBN을 기반으로 도서 정보, 중고 매각가 정보를 보여줄때 쓰인다.

스프레드시트의 ID와 시트 ID가 필요하다.
스프레드시트 ID, 시트ID는 URL에서 얻을수있다.
`https://docs.google.com/spreadsheets/d/{spreadsheet_id}/edit#gid={sheet_id}`

아래와 같은 스프레드시트 URL이 있다고하자.
`https://docs.google.com/spreadsheets/d/13IXCsO7FPjhUmOG0bp08xfZXlusdkSER6b33xPMvV9M/edit#gid=1572089478`
이때 스프레드시트ID는 `13IXCsO7FPjhUmOG0bp08xfZXlusdkSER6b33xPMvV9M`, 시트ID는 `1572089478`이다.

### 준비물 - 접근권한

바코드 스캐너 앱이 스프레드시트에 ISBN을 추가할때는 구글 인증 시스템을 이용하기 때문에 추가작업이 필요없다.
하지만 데이터 갱신자가 스프레드시트에 접근할때는 권한이 필요하다.

[Google API Console](https://console.developers.google.com/apis/)로 접속한다.
적당히 새로운 프로젝트를 생성한다.
`사용자 인증 정보` -> `사용자 인증 정보 만들기` -> `서비스 계정 키`를 통해 새로운 서비스 계정키를 만든다.
서비스 계정은 대충 아무거나 골라도 상관없고 키 유형을 JSON으로 해서 키를 받는다.
이때 어떤 서비스 계정을 선택했는지는 기억한다.

`IAM 및 관리자` -> `서비스 계정`을 찾아서 들어간다.
이전에 선택한 서비스 계정 ID의 이메일 주소를 알아낸다.
스프레드시트의 공유 설정에 들어가서 해당 이메일을 수정가능 권한으로 추가한다

### 준비물 - 네이버 검색 API
ISBN으로 도서 정보를 검색할때는 네이버 검색 API를 이용한다.
[NAVER Developers](https://developers.naver.com)에서 네이버 검색 API를 신청한다.
client id, client secret를 확보한다.


### 바코드 스캐너

#### 빌드 절차
1. `/barcode-reader` 를 Android Studio 로 연다.
2. `app/src/main/java/so/libsora/fumikascanner/MainActivity.java`를 연다.
3. `static String spreadsheetId = "13IXCsO7FPjhUmOG0bp08xfZXlusdkSER6b33xPMvV9M";`를 자신의 스프레드시트ID로 교체한다.
4. 빌드

#### 사용법
1. 앱 이름은 `Fumika Scanner`이다.

![android icon](https://raw.githubusercontent.com/if1live/fumika-scanner/master/document/android-icon.png)

2. `READ BARCODE`를 누르면 바코드를 찍을수 있다. 이 상태에서 바코드를 찍는다.

![android icon](https://raw.githubusercontent.com/if1live/fumika-scanner/master/document/android-init.png)

3. `READY: {ISBN}` 버튼을 누른다. 찍은 바코드를 구글 스프레드시트에 업로드하게된다.

![android icon](https://raw.githubusercontent.com/if1live/fumika-scanner/master/document/android-isbn.png)

4. 바코드가 스프레드시트에 추가되었는지 확인한다.

![android icon](https://raw.githubusercontent.com/if1live/fumika-scanner/master/document/sheet-isbn-sample.png)

5. 문제가 없다면 바코드 찍고 업로드하는 과정을 반복한다.


### 데이터 갱신자

#### 빌드 절차

``` bash
cd service
go get -v ./...
go build
```

#### 설정파일

`service/config.sample.json`을 복사해서 `service/config.json`을 만든다.

```json
{
  "naver_client_id" : "naver-client-id",
  "naver_client_secret" : "naver-client-secret",
  "spreadsheet_id" : "spreadsheet_id",
  "sheet_id" : 0
}
```

naver_client_id, naver_client_secret에는 네이버 검색 API에서 얻은 client id, client secret를 넣어준다.
spreadsheet_id에는 스프레드시트ID를 넣는다.
sheet_id에는 데이터 갱신자가 접근할 스프레드시트ID를 넣는다.

Google API Console에서 얻은 json 파일을 다음 위치에 넣어준다.
`service/client_secret.json`

#### 사용법
1. ISBN 목록을 데이터 갱신자가 사용할 시트에 넣어준다. A2 이후에 넣어준다. 첫번째행은 필드로 예약되어있다.
2. `./service`
3. 스프레드시트의 내용이 갱신된 것을 확인한다. 제목, 출판사, 저자, 가격, 중고매각가가 채워져있을것이다.

![android icon](https://raw.githubusercontent.com/if1live/fumika-scanner/master/document/sheet-info-sample.png)

4. 행에서 ISBN을 제외한 내용을 지우고 `./service`를 실행하면 정보를 갱신한다. 중고 매각가만 갱신하고싶은 경우 `skip`를 지운다.
