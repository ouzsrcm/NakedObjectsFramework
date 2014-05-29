﻿module DomainServices7
open NUnit.Framework
open RestfulObjects.Mvc
open NakedObjects.Surface
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.IO
open Newtonsoft.Json.Linq
open System.Web
open System
open RestfulObjects.Snapshot.Utility 
open RestfulObjects.Snapshot.Constants
open System.Web.Http
open System.Collections.Generic
open System.Linq
open RestTestFunctions
// open System.Json

let getExpected() = 

    let sName1 = ttc "RestfulObjects.Test.Data.RestDataRepository"
    let sName2 = ttc "RestfulObjects.Test.Data.WithActionService"
    let sName3 = ttc "RestfulObjects.Test.Data.ContributorService"

    let srvRel1 = RelValues.Service + makeParm RelParamValues.ServiceId sName1
    let srvRel2 = RelValues.Service + makeParm RelParamValues.ServiceId sName2
    let srvRel3 = RelValues.Service + makeParm RelParamValues.ServiceId sName3

    let simpleLinks = TArray([ TObjectJson(makeLinkPropWithMethodAndTypes "GET" RelValues.Self SegmentValues.Services RepresentationTypes.List "" (ttc "System.Object") true);
                               TObjectJson(makeGetLinkProp RelValues.Up   SegmentValues.HomePage RepresentationTypes.HomePage "")])

    let formalLinks = TArray([ TObjectJson(makeLinkPropWithMethodAndTypes "GET" RelValues.Self SegmentValues.Services RepresentationTypes.List "" (ttc "System.Object") false);
                               TObjectJson(makeGetLinkProp RelValues.Up   SegmentValues.HomePage RepresentationTypes.HomePage "")])

    let value = TArray([ TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("Rest Data Repository")) :: makeGetLinkProp srvRel1 (sprintf "services/%s" sName1) RepresentationTypes.Object sName1);
                         TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("With Action Service"))  :: makeGetLinkProp srvRel2 (sprintf "services/%s" sName2) RepresentationTypes.Object sName2);
                         TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("Contributor Service"))  :: makeGetLinkProp srvRel3 (sprintf "services/%s" sName3) RepresentationTypes.Object sName3)])

    let formalValue = TArray([ TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("Rest Data Repository")) :: makeLinkPropWithMethodAndTypes "GET" srvRel1 (sprintf "services/%s" sName1) RepresentationTypes.Object sName1 "" false);
                               TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("With Action Service"))  :: makeLinkPropWithMethodAndTypes "GET" srvRel2 (sprintf "services/%s" sName2) RepresentationTypes.Object sName2 "" false);
                               TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("Contributor Service"))  :: makeLinkPropWithMethodAndTypes "GET" srvRel3 (sprintf "services/%s" sName3) RepresentationTypes.Object sName3 "" false)])


    let simpleExpected = [ TProperty(JsonPropertyNames.Links, simpleLinks);
                           TProperty(JsonPropertyNames.Value, value);
                           TProperty(JsonPropertyNames.Extensions, TObjectJson([])) ] 

    let formalExpected = [ TProperty(JsonPropertyNames.Links, formalLinks);
                           TProperty(JsonPropertyNames.Value, formalValue);
                           TProperty(JsonPropertyNames.Extensions, TObjectJson([])) ] 

    (simpleExpected, formalExpected) 


let GetDomainServices(api : RestfulObjectsControllerBase) = 
        let url = testRoot + SegmentValues.Services 
        let args = CreateReservedArgs ""
        api.Request <- jsonGetMsg(url)
        let result = api.GetServices(args)
        let jsonResult = readSnapshotToJson result
        let parsedResult = JObject.Parse(jsonResult)

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.List, "", ttc "System.Object", true), result.Content.Headers.ContentType)
        assertNonExpiringCache result 
        let expected = fst (getExpected())
        compareObject expected parsedResult

let GetDomainServicesFormal(api : RestfulObjectsControllerBase) = 
        let argS =  "x-ro-domain-model=formal"
        let url = sprintf "%s?%s" (testRoot + SegmentValues.Services) argS
        let args = CreateReservedArgs argS
        api.Request <- jsonGetMsg(url)
        let result = api.GetServices(args)
        let jsonResult = readSnapshotToJson result
        let parsedResult = JObject.Parse(jsonResult)

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.List, "", ttc "System.Object", false), result.Content.Headers.ContentType)
        assertNonExpiringCache result 
        let expected = snd (getExpected())
        compareObject expected parsedResult

let GetDomainServicesWithMediaType(api : RestfulObjectsControllerBase) = 
        let url = testRoot + SegmentValues.Services 
        let msg = jsonGetMsg(url)
        let args = CreateReservedArgs ""
        msg.Headers.Accept.Single().Parameters.Add(new NameValueHeaderValue ("profile", (makeProfile RepresentationTypes.List)))
        api.Request <- msg
        let result = api.GetServices(args)
        let jsonResult = readSnapshotToJson result
        let parsedResult = JObject.Parse(jsonResult)

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.List, "", ttc "System.Object", true), result.Content.Headers.ContentType)
        assertNonExpiringCache result 
        let expected = fst (getExpected())
        compareObject expected parsedResult

 // 406   
let NotAcceptableGetDomainServices(api : RestfulObjectsControllerBase) = 
    let url = testRoot + SegmentValues.Services 
    let msg = jsonGetMsg(url)
    msg.Headers.Accept.Single().Parameters.Add(new NameValueHeaderValue ("profile", (makeProfile RepresentationTypes.User)))

    try 
        let args = CreateReservedArgs ""
        api.Request <- msg
        let result = api.GetServices(args)
        Assert.Fail("expect exception")
    with 
        | :? HttpResponseException as ex -> Assert.AreEqual(HttpStatusCode.NotAcceptable, ex.Response.StatusCode)