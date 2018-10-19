module React.EventTimeline

open Fable.Core
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import.React

type IEventProps =
    | Title of ReactElement
    | CreatedAt of string
    | Subtitle of string
    | IconColor of string
    | Icon of ReactElement
    interface IHTMLProp

let timelineEvent (props : IHTMLProp list) children =
    ofImport "TimelineEvent" "react-event-timeline" (keyValueList CaseRules.LowerFirst props) children
let timeline children = ofImport "Timeline" "react-event-timeline" (keyValueList CaseRules.LowerFirst []) children
