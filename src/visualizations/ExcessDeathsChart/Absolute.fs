module ExcessDeathsChart.Absolute

open Fable.Core.JsInterop
open Highcharts

open Types
open Browser

let colors = {|
    CurrentYear = "#a483c7"
    BaselineYear = "#d5d5d5"
|}

let renderChartOptions (data : WeeklyDeathsData) =
    let series =
        data
        |> List.groupBy (fun dp -> dp.Year)
        |> List.map (fun (year, data) ->
            let seriesData =
                data
                |> List.map (fun dp ->
                    {| x = dp.Week
                       y = dp.Deceased
                       name = (I18N.tOptions "charts.excessDeaths" {| dateFrom = dp.WeekStartDate ; dateTo = dp.WeekEndDate |})?weekDate
                    |} |> pojo)
                |> List.toArray
            {| ``type`` = "line"
               name = year
               animation = false
               showInLegend = year = System.DateTime.Now.Year
               data = seriesData
               color = if year = System.DateTime.Now.Year then colors.CurrentYear else colors.BaselineYear
               lineWidth = if year = System.DateTime.Now.Year then 2 else 1
               marker = {| enabled = true ; symbol = "circle" ; radius = 2 |} |> pojo
            |} |> pojo)
        |> List.toArray

    {| title = ""
       xAxis = {| labels = {| formatter = fun (x) -> sprintf "%s %s" (I18N.t "week") x?value |} |> pojo |}
       yAxis = {| min = 0 ; title = {| text = None |} ; opposite = true |}
       tooltip = {| formatter = fun () -> sprintf "%s<br>%s: <b>%d</b>" jsThis?series?name jsThis?key jsThis?y |} |> pojo
       series = series
       credits =
        {| enabled = true
           text = sprintf "%s: %s"
                (I18N.t "charts.common.dataSource")
                (I18N.tOptions ("charts.common.dsMNZ") {| context = localStorage.getItem ("contextCountry") |})
           href = "https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-notranje-zadeve/" |} |> pojo
    |} |> pojo
