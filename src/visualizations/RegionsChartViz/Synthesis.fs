﻿module RegionsChartViz.Synthesis

open Highcharts
open RegionsChartViz.Analysis
open Types

type RegionsChartConfig = {
    RelativeTo: MetricRelativeTo
    ChartTextsGroup: string
}

type RegionRenderingConfiguration =
    { Key : string
      Color : string
      Visible : bool }

type RegionsChartState =
    {
      ChartConfig: RegionsChartConfig
      ScaleType : ScaleType
      MetricType : MetricType
      Data : RegionsData
      Regions : Region list
      RegionsConfig : RegionRenderingConfiguration list
      RangeSelectionButtonIndex: int
    }

type RegionSeries = {
    Values: (JsTimestamp * float)[]
}

let visibleRegions state =
    state.RegionsConfig
    |> List.filter (fun regionConfig -> regionConfig.Visible)

let renderRegionPoint state regionConfig (point : RegionsDataPoint) =
    let ts = point.Date |> jsTime12h
    let region =
        point.Regions
        |> List.find (fun reg -> reg.Name = regionConfig.Key)

    let municipalityMetricValue muni =
        match state.MetricType with
        | ActiveCases -> muni.ActiveCases
        | ConfirmedCases -> muni.ConfirmedToDate
        | NewCases7Days -> muni.ConfirmedToDate
        | MetricType.Deceased -> muni.DeceasedToDate
        |> Option.defaultValue 0

    let totalSum =
        region.Municipalities
        |> Seq.sumBy municipalityMetricValue
        |> float

    let finalValue =
        match state.ChartConfig.RelativeTo with
        | Absolute -> totalSum
        | Pop100k ->
            let regionPopulation =
                Utils.Dictionaries.regions.[region.Name].Population
                |> Option.get
                |> float

            let regionPopBy100k = regionPopulation / 100000.0
            totalSum / regionPopBy100k

    ts, finalValue

let regionSeries state regionConfig: RegionSeries =
    let renderPoint = renderRegionPoint state regionConfig

    let seriesValues =
        state.Data
        |> Seq.map renderPoint
        |> Array.ofSeq

    { Values = seriesValues }

let allSeries state =
    visibleRegions state
    |> List.map (fun regionConfig ->
        let regionSeries = regionSeries state regionConfig

        {|
            name = I18N.tt "region" regionConfig.Key
            color = regionConfig.Color
            data = regionSeries.Values
        |}
        |> pojo
    )
    |> List.toArray
