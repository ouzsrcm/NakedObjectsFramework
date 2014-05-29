﻿module Objects9
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
open System.Collections.Generic
open System.Linq
open RestTestFunctions
// open System.Json


let GetMostSimpleTransientObject(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientMostSimple"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let result = api.PostInvokeOnService(oType, pid, args)
        let jsonResult = readSnapshotToJson result
        let parsedResult = JObject.Parse(jsonResult)
      
        let roType = ttc "RestfulObjects.Test.Data.MostSimple"
         
        let argsMembers = TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0))]))]))
                
        let args = TProperty(JsonPropertyNames.Arguments, TObjectJson([argsMembers]))

        let resultObject =  TObjectJson([ TProperty(JsonPropertyNames.Title, TObjectVal("0") );
                                          TProperty(JsonPropertyNames.Links, TArray( [ TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" roType)  RepresentationTypes.DomainType "");
                                                                                       //TObjectJson(makeIconLink());
                                                                                       TObjectJson(args :: makePostLinkProp RelValues.Persist  (sprintf "objects/%s" roType)  RepresentationTypes.Object "")
                                                                        ]));
                                          TProperty(JsonPropertyNames.Members, TObjectJson( [ TProperty("Id", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                              TProperty(JsonPropertyNames.Id, TObjectVal("Id"));
                                                                                                                              TProperty(JsonPropertyNames.Value, TObjectVal(0));
                                                                                                                              TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                                                                                                                             
                                                                                                                              TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "Id") RepresentationTypes.PropertyDescription "")])); 
                                                                                                                              TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.FriendlyName, TObjectVal("Id"));
                                                                                                                                                                                   TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                                                                                                                   TProperty(JsonPropertyNames.ReturnType, TObjectVal("number")); 
                                                                                                                                                                                   TProperty(JsonPropertyNames.Format, TObjectVal("integer"));
                                                                                                                                                                                   TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));                                                                                                                                                 
                                                                                                                                                                                   TProperty(JsonPropertyNames.Optional, TObjectVal(false))]));]))]));                                                                                                                
                                          TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(roType));
                                                                                               TProperty(JsonPropertyNames.FriendlyName, TObjectVal("Most Simple"));
                                                                                               TProperty(JsonPropertyNames.PluralName, TObjectVal("Most Simples"));
                                                                                               TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                               TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) ) ])

        let expected =  [ TProperty(JsonPropertyNames.Links, TArray([]));
                          TProperty(JsonPropertyNames.ResultType, TObjectVal(ResultTypes.Object));
                          TProperty(JsonPropertyNames.Result, resultObject);
                          TProperty(JsonPropertyNames.Extensions, TObjectJson([]))] 

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.ActionResult, roType, "", true), result.Content.Headers.ContentType)
        assertTransactionalCache  result 
        compareObject expected parsedResult

let GetMostSimpleTransientObjectSimpleOnly(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientMostSimple"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let parms =  new JObject (new JProperty("x-ro-domain-model", "simple"))

        let args = CreateArgMap parms

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) (parms.ToString())
        let result = api.PostInvokeOnService(oType, pid, args)
        let jsonResult = readSnapshotToJson result
        let parsedResult = JObject.Parse(jsonResult)
      
        let roType = ttc "RestfulObjects.Test.Data.MostSimple"
   
        let argsMembers = TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0))]))]))
        let args = TProperty(JsonPropertyNames.Arguments, TObjectJson([argsMembers]))

        let resultObject =  TObjectJson([ TProperty(JsonPropertyNames.Title, TObjectVal("0") );
                                          TProperty(JsonPropertyNames.Links, TArray( [ //TObjectJson(makeIconLink());
                                                                                       TObjectJson(args :: makePostLinkProp RelValues.Persist  (sprintf "objects/%s" roType)  RepresentationTypes.Object "")
                                                                        ]));
                                          TProperty(JsonPropertyNames.Members, TObjectJson( [ TProperty("Id", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                              TProperty(JsonPropertyNames.Id, TObjectVal("Id"));
                                                                                                                              TProperty(JsonPropertyNames.Value, TObjectVal(0));
                                                                                                                              TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                                                                                                                             
                                                                                                                              TProperty(JsonPropertyNames.Links, TArray([])); 
                                                                                                                              TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.FriendlyName, TObjectVal("Id"));
                                                                                                                                                                                   TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                                                                                                                   TProperty(JsonPropertyNames.ReturnType, TObjectVal("number")); 
                                                                                                                                                                                   TProperty(JsonPropertyNames.Format, TObjectVal("integer"));
                                                                                                                                                                                   TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));                                                                                                                                                 
                                                                                                                                                                                   TProperty(JsonPropertyNames.Optional, TObjectVal(false))]));]))]));                                                                                                                
                                          TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(roType));
                                                                                               TProperty(JsonPropertyNames.FriendlyName, TObjectVal("Most Simple"));
                                                                                               TProperty(JsonPropertyNames.PluralName, TObjectVal("Most Simples"));
                                                                                               TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                               TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) ) ])

        let expected =  [ TProperty(JsonPropertyNames.Links, TArray([]));
                          TProperty(JsonPropertyNames.ResultType, TObjectVal(ResultTypes.Object));
                          TProperty(JsonPropertyNames.Result, resultObject);
                          TProperty(JsonPropertyNames.Extensions, TObjectJson([]))] 

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.ActionResult, roType, "", true), result.Content.Headers.ContentType)
        assertTransactionalCache  result 
        compareObject expected parsedResult

let GetMostSimpleTransientObjectFormalOnly(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientMostSimple"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let parms =  new JObject (new JProperty("x-ro-domain-model", "formal"))

        let args = CreateArgMap parms

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) (parms.ToString())
        let result = api.PostInvokeOnService(oType, pid, args)
        let jsonResult = readSnapshotToJson result
        let parsedResult = JObject.Parse(jsonResult)
      
        let roType = ttc "RestfulObjects.Test.Data.MostSimple"
   
        let argsMembers = TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0))]))]))
        let args = TProperty(JsonPropertyNames.Arguments, TObjectJson([argsMembers]))

        let resultObject =  TObjectJson([ TProperty(JsonPropertyNames.Title, TObjectVal("0") );
                                          TProperty(JsonPropertyNames.Links, TArray( [ TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" roType)  RepresentationTypes.DomainType "");
                                                                                       //TObjectJson(makeIconLink());
                                                                                       TObjectJson(args :: makePostLinkProp RelValues.Persist  (sprintf "objects/%s" roType)  RepresentationTypes.Object "")
                                                                        ]));
                                          TProperty(JsonPropertyNames.Members, TObjectJson( [ TProperty("Id", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                              TProperty(JsonPropertyNames.Id, TObjectVal("Id"));
                                                                                                                              TProperty(JsonPropertyNames.Value, TObjectVal(0));
                                                                                                                              TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                                                                                                                             
                                                                                                                              TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "Id") RepresentationTypes.PropertyDescription "")])); 
                                                                                                                              TProperty(JsonPropertyNames.Extensions, TObjectJson([]));]))]));                                                                                                                
                                          TProperty(JsonPropertyNames.Extensions, TObjectJson([]) ) ])

        let expected =  [ TProperty(JsonPropertyNames.Links, TArray([]));
                          TProperty(JsonPropertyNames.ResultType, TObjectVal(ResultTypes.Object));
                          TProperty(JsonPropertyNames.Result, resultObject);
                          TProperty(JsonPropertyNames.Extensions, TObjectJson([]))] 

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.ActionResult, roType, "", false), result.Content.Headers.ContentType)
        assertTransactionalCache  result 
        compareObject expected parsedResult

let PersistMostSimpleTransientObject(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientMostSimple"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)
       
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
     
        let roType = ttc "RestfulObjects.Test.Data.MostSimple"
        let oid = roType + "/" + ktc "1"
        let args = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))

        let expected = [ TProperty(JsonPropertyNames.DomainType, TObjectVal(roType)); TProperty(JsonPropertyNames.InstanceId, TObjectVal(ktc "1"));
                         TProperty(JsonPropertyNames.Title, TObjectVal("1"));
                         TProperty(JsonPropertyNames.Links, TArray([ TObjectJson(makeGetLinkProp RelValues.Self (sprintf "objects/%s" oid)  RepresentationTypes.Object roType);                                                     
                                                                     TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" roType)  RepresentationTypes.DomainType "");
                                                                     //TObjectJson(makeIconLink()); 
                                                                     TObjectJson(args :: makePutLinkProp RelValues.Update (sprintf "objects/%s" oid)  RepresentationTypes.Object roType) ]));
                         TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("Id", TObjectJson(makeObjectPropertyMember "Id" oid  "Id" (TObjectVal(1)))) ]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(roType));
                                                                              TProperty(JsonPropertyNames.FriendlyName, TObjectVal("Most Simple"));
                                                                              TProperty(JsonPropertyNames.PluralName, TObjectVal("Most Simples"));
                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                              TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) ) ]

        Assert.AreEqual(HttpStatusCode.Created, persistResult.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.Object, roType), persistResult.Content.Headers.ContentType)
        Assert.AreEqual(true, persistResult.Headers.CacheControl.NoCache)
        Assert.AreEqual((sprintf "http://localhost/objects/%s" oid), persistResult.Headers.Location.ToString())
        compareObject expected parsedPersist

let PersistMostSimpleTransientObjectValidateOnly(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientMostSimple"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
         
        let voProp = new JProperty("x-ro-validate-only", true)    
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)
        args.First.Last.AddAfterSelf(voProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
        
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonResult = readSnapshotToJson persistResult
      
        Assert.AreEqual(HttpStatusCode.NoContent, persistResult.StatusCode)
        Assert.AreEqual("", jsonResult)

let PersistMostSimpleTransientObjectSimpleOnly(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientMostSimple"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let simpleProp = new JProperty("x-ro-domain-model", "simple")
        let parms =  new JObject (simpleProp)

        let args = CreateArgMap parms

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) (parms.ToString())
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[0]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)
        args.First.Last.AddAfterSelf(simpleProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  

        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
     
        let oType = ttc "RestfulObjects.Test.Data.MostSimple"
        let oid = oType + "/" + ktc "1"
        let args = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))

        let expected = [ TProperty(JsonPropertyNames.DomainType, TObjectVal(oType));
                         TProperty(JsonPropertyNames.InstanceId, TObjectVal(ktc "1"));
                         TProperty(JsonPropertyNames.Title, TObjectVal("1"));
                         TProperty(JsonPropertyNames.Links, TArray([ TObjectJson(makeGetLinkProp RelValues.Self (sprintf "objects/%s" oid)  RepresentationTypes.Object oType);                                                                                                          
                                                                     //TObjectJson(makeIconLink()); 
                                                                     TObjectJson(args :: makePutLinkProp RelValues.Update (sprintf "objects/%s" oid)  RepresentationTypes.Object oType) ]));
                         TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("Id",TObjectJson(makePropertyMemberSimpleNumber "objects" "Id" oid "Id" ""  "integer" false (TObjectVal(1)))) ]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(oType));
                                                                              TProperty(JsonPropertyNames.FriendlyName, TObjectVal("Most Simple"));
                                                                              TProperty(JsonPropertyNames.PluralName, TObjectVal("Most Simples"));
                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                              TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) ) ]

        Assert.AreEqual(HttpStatusCode.Created, persistResult.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.Object, oType), persistResult.Content.Headers.ContentType)
        Assert.AreEqual(true, persistResult.Headers.CacheControl.NoCache)
        Assert.AreEqual((sprintf "http://localhost/objects/%s" oid), persistResult.Headers.Location.ToString())
        compareObject expected parsedPersist

let PersistMostSimpleTransientObjectFormalOnly(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientMostSimple"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let formalProp = new JProperty("x-ro-domain-model", "formal")
        let parms =  new JObject (formalProp)

        let args = CreateArgMap parms

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) (parms.ToString())
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)
        args.First.Last.AddAfterSelf(formalProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
     
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
     
        let oType = ttc "RestfulObjects.Test.Data.MostSimple"
        let oid = oType + "/" + ktc "1"
        let args = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))

        let expected = [ TProperty(JsonPropertyNames.InstanceId, TObjectVal(ktc "1"));
                         TProperty(JsonPropertyNames.Title, TObjectVal("1"));
                         TProperty(JsonPropertyNames.Links, TArray([ TObjectJson(makeLinkPropWithMethodAndTypes "GET" RelValues.Self (sprintf "objects/%s" oid)  RepresentationTypes.Object oType "" false);   
                                                                     TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" oType)  RepresentationTypes.DomainType "");
                                                                     //TObjectJson(makeIconLink()); 
                                                                     TObjectJson(args :: makeLinkPropWithMethodAndTypes "PUT" RelValues.Update (sprintf "objects/%s" oid)  RepresentationTypes.Object oType "" false) ]));
                         TProperty(JsonPropertyNames.Members, TObjectJson([ TProperty("Id", TObjectJson(makePropertyMemberFormal "objects" "Id" oid (TObjectVal(1)) false)) ]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([]) ) ]

        Assert.AreEqual(HttpStatusCode.Created, persistResult.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.Object, oType, "", false), persistResult.Content.Headers.ContentType)
        Assert.AreEqual(true, persistResult.Headers.CacheControl.NoCache)
        Assert.AreEqual((sprintf "http://localhost/objects/%s" oid), persistResult.Headers.Location.ToString())
        compareObject expected parsedPersist

let GetWithValueTransientObject(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithValue"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let result = api.PostInvokeOnService(oType, pid, args)
        let jsonResult = readSnapshotToJson result
        let parsedResult = JObject.Parse(jsonResult)
      
        let roType = ttc "RestfulObjects.Test.Data.WithValue"
         
        let argsMembers = TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("AChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(3))]));
                                                                            TProperty("AConditionalChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(3))]));
                                                                            TProperty("ADateTimeValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(DateTime.Parse("2012-02-10T00:00:00Z").ToUniversalTime()))]));
                                                                            TProperty("ADisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(103))]));
                                                                            TProperty("AHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(105))]));
                                                                            TProperty("AStringValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal("one hundred four"))]));
                                                                            TProperty("AUserDisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0))]));
                                                                            TProperty("AUserHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0))]));
                                                                            TProperty("AValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(102))]));
                                                                            TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0))])) ]))
        let args = TProperty(JsonPropertyNames.Arguments, TObjectJson([argsMembers]))

        let resultObject =  TObjectJson([ TProperty(JsonPropertyNames.Title, TObjectVal("0") );
                                          TProperty(JsonPropertyNames.Links, TArray( [ TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" roType)  RepresentationTypes.DomainType "");
                                                                                       //TObjectJson(makeIconLink());
                                                                                       TObjectJson(args :: makePostLinkProp RelValues.Persist  (sprintf "objects/%s" roType)  RepresentationTypes.Object "")
                                                                        ]));
                                          TProperty(JsonPropertyNames.Members,TObjectJson( [ TProperty("AChoicesValue", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                                         TProperty(JsonPropertyNames.Id, TObjectVal("AChoicesValue"));
                                                                                                                                         TProperty(JsonPropertyNames.Value, TObjectVal(3));
                                                                                                                                         TProperty(JsonPropertyNames.HasChoices, TObjectVal(true));
                                                                                                                                         TProperty(JsonPropertyNames.Choices, TArray([TObjectVal(1);TObjectVal(2);TObjectVal(3)]));
                                                                                                                                       
                                                                                                                                         TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "AChoicesValue") RepresentationTypes.PropertyDescription "")])); 
                                                                                                                                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.FriendlyName, TObjectVal("A Choices Value"));
                                                                                                                                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                                                                                                                              TProperty(JsonPropertyNames.ReturnType, TObjectVal("number"));
                                                                                                                                                                                              TProperty(JsonPropertyNames.Format, TObjectVal("integer"));  
                                                                                                                                                                                              TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));  
                                                                                                                                                                                              TProperty(JsonPropertyNames.CustomChoices, TObjectJson( [ TProperty("1", TObjectVal(1)); TProperty("2", TObjectVal(2)); TProperty("3", TObjectVal(3))   ]  ));                                                                                                                                                                                                                        
                                                                                                                                                                                              TProperty(JsonPropertyNames.Optional, TObjectVal(false))]));]));
                                                                                              
                                                                                              TProperty("AConditionalChoicesValue", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                                         TProperty(JsonPropertyNames.Id, TObjectVal("AConditionalChoicesValue"));
                                                                                                                                         TProperty(JsonPropertyNames.Value, TObjectVal(3));
                                                                                                                                         TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));                                                                                                                                                                                                                                                                                
                                                                                                                                         TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "AConditionalChoicesValue") RepresentationTypes.PropertyDescription "")])); 
                                                                                                                                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.FriendlyName, TObjectVal("A Conditional Choices Value"));
                                                                                                                                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                                                                                                                              TProperty(JsonPropertyNames.ReturnType, TObjectVal("number"));
                                                                                                                                                                                              TProperty(JsonPropertyNames.Format, TObjectVal("integer"));  
                                                                                                                                                                                              TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));                                                                                                                                                                                                                                                                                                                                                               
                                                                                                                                                                                              TProperty(JsonPropertyNames.Optional, TObjectVal(false))]));]));

                                                                                              TProperty("ADateTimeValue", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                                          TProperty(JsonPropertyNames.Id, TObjectVal("ADateTimeValue"));
                                                                                                                                          TProperty(JsonPropertyNames.Value, TObjectVal(DateTime.Parse("2012-02-10T00:00:00Z").ToUniversalTime()));
                                                                                                                                          TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                                                                                                                                          
                                                                                                                                          TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "ADateTimeValue") RepresentationTypes.PropertyDescription "")])); 
                                                                                                                                          TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.Description, TObjectVal("A datetime value for testing"));
                                                                                                                                                                                               TProperty(JsonPropertyNames.ReturnType, TObjectVal("string"));
                                                                                                                                                                                               TProperty(JsonPropertyNames.MaxLength, TObjectVal(0));
                                                                                                                                                                                               TProperty(JsonPropertyNames.Pattern, TObjectVal(""));
                                                                                                                                                                                               TProperty(JsonPropertyNames.Format, TObjectVal("date-time"));
                                                                                                                                                                                               TProperty(JsonPropertyNames.FriendlyName, TObjectVal("A Date Time Value"));
                                                                                                                                                                                               TProperty(JsonPropertyNames.MemberOrder, TObjectVal(4));
                                                                                                                                                                                               TProperty(JsonPropertyNames.CustomMask, TObjectVal("d"));
                                                                                                                                                                                               TProperty(JsonPropertyNames.Optional, TObjectVal(true))]));]));
                                                                                              TProperty("ADisabledValue", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                                          TProperty(JsonPropertyNames.Id, TObjectVal("ADisabledValue"));
                                                                                                                                          TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable"));

                                                                                                                                          TProperty(JsonPropertyNames.Value, TObjectVal(103));
                                                                                                                                          TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                                                                                                                                         
                                                                                                                                          TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "ADisabledValue") RepresentationTypes.PropertyDescription "")])); 
                                                                                                                                          TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.FriendlyName, TObjectVal("A Disabled Value"));
                                                                                                                                                                                               TProperty(JsonPropertyNames.ReturnType, TObjectVal("number"));
                                                                                                                                                                                               TProperty(JsonPropertyNames.Format, TObjectVal("integer"));
                                                                                                                                                                                               TProperty(JsonPropertyNames.Description, TObjectVal(""));        
                                                                                                                                                                                               TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));                                                                                                                                                                                                                                                                                                      
                                                                                                                                                                                               TProperty(JsonPropertyNames.Optional, TObjectVal(false))]));]));
                                                                                              TProperty("AStringValue", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                                        TProperty(JsonPropertyNames.Id, TObjectVal("AStringValue"));
                                                                                                                                        TProperty(JsonPropertyNames.Value, TObjectVal("one hundred four"));
                                                                                                                                        TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                                                                                                                                       
                                                                                                                                        TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "AStringValue") RepresentationTypes.PropertyDescription "")]));  
                                                                                                                                        TProperty(JsonPropertyNames.Extensions, TObjectJson([
                                                                                                                                                                                             TProperty(JsonPropertyNames.Description, TObjectVal("A string value for testing"));
                                                                                                                                                                                             TProperty(JsonPropertyNames.ReturnType, TObjectVal("string"));
                                                                                                                                                                                             TProperty(JsonPropertyNames.Format, TObjectVal("string"));
                                                                                                                                                                                             TProperty(JsonPropertyNames.MaxLength, TObjectVal(101));
                                                                                                                                                                                             TProperty(JsonPropertyNames.Pattern, TObjectVal("[A-Z]"));
                                                                                                                                                                                             TProperty(JsonPropertyNames.FriendlyName, TObjectVal("A String Value"));
                                                                                                                                                                                             TProperty(JsonPropertyNames.MemberOrder, TObjectVal(3));
                                                                                                                                                                                             TProperty(JsonPropertyNames.Optional, TObjectVal(true))]));]));

                                                                                                                                                                      
                                                                                              TProperty("AUserDisabledValue", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                                              TProperty(JsonPropertyNames.Id, TObjectVal("AUserDisabledValue"));
                                                                                                                                              TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Not authorized to edit"));
                                                                                                                                              TProperty(JsonPropertyNames.Value, TObjectVal(0));
                                                                                                                                              TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                                                                                                                                             
                                                                                                                                              TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "AUserDisabledValue") RepresentationTypes.PropertyDescription "")])); 
                                                                                                                                              TProperty(JsonPropertyNames.Extensions, TObjectJson([
                                                                                                                                                                                                     TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                                                                                                                                     TProperty(JsonPropertyNames.ReturnType, TObjectVal("number")); 
                                                                                                                                                                                                     TProperty(JsonPropertyNames.Format, TObjectVal("integer"));                                                                                                                                                                 
                                                                                                                                                                                                     TProperty(JsonPropertyNames.FriendlyName, TObjectVal("A User Disabled Value"));
                                                                                                                                                                                                     TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));
                                                                                                                                                                                                     TProperty(JsonPropertyNames.Optional, TObjectVal(false))]));]));
                                                                                              TProperty("AValue", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                                  TProperty(JsonPropertyNames.Id, TObjectVal("AValue"));
                                                                                                                                  TProperty(JsonPropertyNames.Value, TObjectVal(102));
                                                                                                                                  TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                                                                                                                                 
                                                                                                                                  TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "AValue") RepresentationTypes.PropertyDescription "")])); 
                                                                                                                                  TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.FriendlyName, TObjectVal("A Value"));
                                                                                                                                                                                       TProperty(JsonPropertyNames.ReturnType, TObjectVal("number")); 
                                                                                                                                                                                       TProperty(JsonPropertyNames.Format, TObjectVal("integer"));
                                                                                                                                                                                       TProperty(JsonPropertyNames.Description, TObjectVal(""));       
                                                                                                                                                                                       TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));   
                                                                                                                                                                                       TProperty(JsonPropertyNames.PresentationHint, TObjectVal("class3 class4"))                                                                                                                                         
                                                                                                                                                                                       TProperty(JsonPropertyNames.Optional, TObjectVal(false))]));]));
                                                                                              TProperty("Id", TObjectJson(  [ TProperty(JsonPropertyNames.MemberType, TObjectVal(MemberTypes.Property) );
                                                                                                                              TProperty(JsonPropertyNames.Id, TObjectVal("Id"));
                                                                                                                              TProperty(JsonPropertyNames.Value, TObjectVal(0));
                                                                                                                              TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                                                                                                                             
                                                                                                                              TProperty(JsonPropertyNames.Links, TArray([TObjectJson( makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" roType "Id") RepresentationTypes.PropertyDescription "")])); 
                                                                                                                              TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.FriendlyName, TObjectVal("Id"));
                                                                                                                                                                                   TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                                                                                                                   TProperty(JsonPropertyNames.ReturnType, TObjectVal("number")); 
                                                                                                                                                                                   TProperty(JsonPropertyNames.Format, TObjectVal("integer"));
                                                                                                                                                                                   TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));                                                                                                                                               
                                                                                                                                                                                   TProperty(JsonPropertyNames.Optional, TObjectVal(false))]));]));]));                                                                                                                
                                          TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(roType));
                                                                                               TProperty(JsonPropertyNames.FriendlyName, TObjectVal("With Value"));
                                                                                               TProperty(JsonPropertyNames.PluralName, TObjectVal("With Values"));
                                                                                               TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                               TProperty(JsonPropertyNames.PresentationHint, TObjectVal("class1 class2"));
                                                                                               TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) ) ])

        let expected =  [ TProperty(JsonPropertyNames.Links, TArray([]));
                          TProperty(JsonPropertyNames.ResultType, TObjectVal(ResultTypes.Object));
                          TProperty(JsonPropertyNames.Result, resultObject);
                          TProperty(JsonPropertyNames.Extensions, TObjectJson([]))] 

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.ActionResult, roType, "", true), result.Content.Headers.ContentType)
        assertTransactionalCache  result 
        compareObject expected parsedResult

let GetWithReferenceTransientObject(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithReference"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let result = api.PostInvokeOnService(oType, pid, args)
        let jsonResult = readSnapshotToJson result
        let parsedResult = JObject.Parse(jsonResult)
      
        let mst = ttc "RestfulObjects.Test.Data.MostSimple"
        let roType = ttc "RestfulObjects.Test.Data.WithReference"
        let oid = (sprintf "objects/%s/%s" mst (ktc "1" ))
          
        let argsMembers = TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("AChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson([TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(oid)))]))]));                                                          
                                                                            TProperty("AConditionalChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson([TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(oid)))]))]));                                                                                                                                      
                                                                            TProperty("ADisabledReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson([TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(oid)))]))]));   
                                                                            TProperty("AHiddenReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))])); 
                                                                            TProperty("ANullReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));                                                                                                                                
                                                                            TProperty("AReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson([TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(oid)))]))]));         
                                                                            TProperty("AnAutoCompleteReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson([TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(oid)))]))]));                                                          
                                                                            TProperty("AnEagerReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson([TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(oid)))]))]));         
                                                                            TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0))])) ]))
        let args = TProperty(JsonPropertyNames.Arguments, TObjectJson([argsMembers]))


        let disabledValue = TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable")) :: (makeObjectPropertyMember "ADisabledValue" oid "A Disabled Value" (TObjectVal("200")))
        

        let valueRel1 = RelValues.Value + makeParm RelParamValues.Property "AChoicesReference"
        let valueRel2 = RelValues.Value + makeParm RelParamValues.Property "ADisabledReference"
        let valueRel3 = RelValues.Value + makeParm RelParamValues.Property "AReference"
        let valueRel4 = RelValues.Value + makeParm RelParamValues.Property "AnEagerReference"
        let valueRel5 = RelValues.Value + makeParm RelParamValues.Property "AnAutoCompleteReference"
        let valueRel6 = RelValues.Value + makeParm RelParamValues.Property "AConditionalChoicesReference"

        let args1 = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))
        let msObj    = [ TProperty(JsonPropertyNames.DomainType, TObjectVal(mst)); TProperty(JsonPropertyNames.InstanceId, TObjectVal(ktc "1"));
                         TProperty(JsonPropertyNames.Title, TObjectVal("1"));
                         TProperty(JsonPropertyNames.Links, TArray([ TObjectJson(makeGetLinkProp RelValues.Self  oid  RepresentationTypes.Object mst);
                                                                     TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" mst)  RepresentationTypes.DomainType "");
                                                                     //TObjectJson(makeIconLink()); 
                                                                     TObjectJson(args1 :: makePutLinkProp RelValues.Update  oid  RepresentationTypes.Object mst) ]));
                         TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("Id", TObjectJson(makeObjectPropertyMember "Id" (mst + "/" + ktc "1")  "Id" (TObjectVal(1)))) ]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(mst));
                                                                              TProperty(JsonPropertyNames.FriendlyName, TObjectVal("Most Simple"));
                                                                              TProperty(JsonPropertyNames.PluralName, TObjectVal("Most Simples"));
                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                              TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) ) ]


        let val1 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel1 (sprintf "objects/%s/%s" mst (ktc "1"))  RepresentationTypes.Object mst)
        let val2 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel2 (sprintf "objects/%s/%s" mst (ktc "1"))  RepresentationTypes.Object mst)
        let val3 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel3 (sprintf "objects/%s/%s" mst (ktc "1"))  RepresentationTypes.Object mst)
        let val4 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeLinkPropWithMethodValue "GET" valueRel4 (sprintf "objects/%s/%s" mst (ktc "1"))  RepresentationTypes.Object mst (TObjectJson(msObj))  )
        let val5 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel5 (sprintf "objects/%s/%s" mst (ktc "1"))  RepresentationTypes.Object mst)
        let val6 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel6 (sprintf "objects/%s/%s" mst (ktc "1"))  RepresentationTypes.Object mst)


        let arguments = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("AChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AConditionalChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));  
                                                                            TProperty("ANullReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AnAutoCompleteReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AnEagerReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))

    


        let resultObject =  TObjectJson([ TProperty(JsonPropertyNames.Title, TObjectVal("0") );
                                          TProperty(JsonPropertyNames.Links, TArray( [ TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" roType)  RepresentationTypes.DomainType "");
                                                                                       //TObjectJson(makeIconLink());
                                                                                       TObjectJson(args :: makePostLinkProp RelValues.Persist  (sprintf "objects/%s" roType)  RepresentationTypes.Object "") ]));
                                          TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("AChoicesReference", TObjectJson(makePropertyMemberShortNoDetails "objects" "AChoicesReference" roType "A Choices Reference" "" mst false val1));
                                                                                            TProperty("AConditionalChoicesReference", TObjectJson(makePropertyMemberShortNoDetails "objects" "AConditionalChoicesReference" roType "A Conditional Choices Reference" "" mst false val6));
                                                                                            TProperty("ADisabledReference", TObjectJson(TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable")) :: (makePropertyMemberShortNoDetails "objects" "ADisabledReference" roType "A Disabled Reference" "" mst false val2 )));
                                                                                            TProperty("ANullReference", TObjectJson(makePropertyMemberShortNoDetails "objects" "ANullReference" roType "A Null Reference" "" mst true (TObjectVal(null))));
                                                                                            TProperty("AReference", TObjectJson(makePropertyMemberShortNoDetails "objects" "AReference" roType "A Reference" "" mst false val3));
                                                                                            TProperty("AnAutoCompleteReference", TObjectJson(makePropertyMemberShortNoDetails "objects" "AnAutoCompleteReference" roType "An Auto Complete Reference" "" mst false val5));
                                                                                            TProperty("AnEagerReference", TObjectJson(makePropertyMemberShortNoDetails "objects" "AnEagerReference" roType "An Eager Reference" "" mst false val4));
                                                                                            TProperty("Id", TObjectJson(makePropertyMemberFullNoDetails "objects"  "Id" roType  "Id" "" false (TObjectVal(0)))) ]));
                                          TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(roType));
                                                                                               TProperty(JsonPropertyNames.FriendlyName, TObjectVal("With Reference"));
                                                                                               TProperty(JsonPropertyNames.PluralName, TObjectVal("With References"));
                                                                                               TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                               TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) ) ])

        let expected =  [ TProperty(JsonPropertyNames.Links, TArray([]));
                          TProperty(JsonPropertyNames.ResultType, TObjectVal(ResultTypes.Object));
                          TProperty(JsonPropertyNames.Result, resultObject);
                          TProperty(JsonPropertyNames.Extensions, TObjectJson([]))] 

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.ActionResult, roType, "", true), result.Content.Headers.ContentType)
        assertTransactionalCache  result 

        let ps = parsedResult.ToString()

        compareObject expected parsedResult


let GetWithCollectionTransientObject(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithCollection"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let result = api.PostInvokeOnService(oType, pid, args)
        let jsonResult = readSnapshotToJson result
        let parsedResult = JObject.Parse(jsonResult)
      
        let roType = ttc "RestfulObjects.Test.Data.WithCollection"
        let mst = ttc "RestfulObjects.Test.Data.MostSimple"
        let mstv = ttc "RestfulObjects.Test.Data.MostSimpleViewModel"
        let roid = sprintf "%s/0" roType
        let oid = (sprintf "objects/%s/%s" mst (ktc "1"))
        let oid2 = (sprintf "objects/%s/%s" mst (ktc "2"))
        let oid3 = (sprintf "objects/%s/%s" mstv (ktc "1"))
        let oid4 = (sprintf "objects/%s/%s" mstv (ktc "2"))
   
         
        let argsMembers = TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0))]))]))
        let args = TProperty(JsonPropertyNames.Arguments, TObjectJson([argsMembers]))

        let disabledValue = TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable")) :: (makeObjectPropertyMember "ADisabledValue" oid "A Disabled Value" (TObjectVal("200")))
        

        let valueRel = RelValues.Value + makeParm RelParamValues.Collection "ACollection"

        let val1 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel oid  RepresentationTypes.Object mst)
        let val2 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("2")) :: makeGetLinkProp valueRel oid2  RepresentationTypes.Object mst)
 
        let valueRel = RelValues.Value + makeParm RelParamValues.Collection "ADisabledCollection"

        let val3 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel oid  RepresentationTypes.Object mst)
        let val4 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("2")) :: makeGetLinkProp valueRel oid2  RepresentationTypes.Object mst)
 
        let valueRel = RelValues.Value + makeParm RelParamValues.Collection "ASet"

        let val5 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel oid  RepresentationTypes.Object mst)
        let val6 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("2")) :: makeGetLinkProp valueRel oid2  RepresentationTypes.Object mst)
 
        let valueRel = RelValues.Value + makeParm RelParamValues.Collection "ACollectionViewModels"

        let val7 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel oid3  RepresentationTypes.Object mstv)
        let val8 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("2")) :: makeGetLinkProp valueRel oid4  RepresentationTypes.Object mstv)

        let valueRel = RelValues.Value + makeParm RelParamValues.Collection "AnEagerCollection"

        let val9 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel oid  RepresentationTypes.Object mst)
        let val10 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("2")) :: makeGetLinkProp valueRel oid2  RepresentationTypes.Object mst)

        let resultObject = TObjectJson([ TProperty(JsonPropertyNames.Title, TObjectVal("0"));
                                         TProperty(JsonPropertyNames.Links, TArray( [ TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" roType)  RepresentationTypes.DomainType "");
                                                                                      //TObjectJson(makeIconLink());
                                                                                      TObjectJson(args :: makePostLinkProp RelValues.Persist  (sprintf "objects/%s" roType)  RepresentationTypes.Object "") ]));
                                         TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("ACollection", TObjectJson(makeCollectionMemberNoValue  "ACollection" roid "A Collection" "" "list"  2 mst "Most Simples" ));
                                                                                           TProperty("ACollectionViewModels", TObjectJson(makeCollectionMemberNoValue  "ACollectionViewModels" roid "A Collection View Models" "" "list"  2 mstv "Most Simple View Models"  ));
                                                                                           TProperty("ADisabledCollection", TObjectJson( (makeCollectionMemberNoValue  "ADisabledCollection" roid "A Disabled Collection" "" "list"  2 mst "Most Simples" )));
                                                                                           TProperty("AnEmptyCollection", TObjectJson(makeCollectionMemberNoValue  "AnEmptyCollection" roid "An Empty Collection" "an empty collection for testing" "list"  0 mst "Most Simples" ));
                                                                                           TProperty("AnEagerCollection", TObjectJson(makeCollectionMemberNoValue  "AnEagerCollection" roid "An Eager Collection" "" "list"  2 mst "Most Simples" ));
                                                                                           TProperty("ASet", TObjectJson(makeCollectionMemberNoValue  "ASet" roid "A Set" "" "set"  2 mst "Most Simples" ));
                                                                                           TProperty("AnEmptySet", TObjectJson(makeCollectionMemberNoValue  "AnEmptySet" roid "An Empty Set" "an empty set for testing" "set"  0 mst "Most Simples" ));
                                                                                           TProperty("Id", TObjectJson(makePropertyMemberFullNoDetails "objects"  "Id" roType  "Id" "" false (TObjectVal(0)))) ]));
                                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(roType));
                                                                                              TProperty(JsonPropertyNames.FriendlyName, TObjectVal("With Collection"));
                                                                                              TProperty(JsonPropertyNames.PluralName, TObjectVal("With Collections"));
                                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                                              TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) )])

        let expected =  [ TProperty(JsonPropertyNames.Links, TArray([]));
                          TProperty(JsonPropertyNames.ResultType, TObjectVal(ResultTypes.Object));
                          TProperty(JsonPropertyNames.Result, resultObject);
                          TProperty(JsonPropertyNames.Extensions, TObjectJson([]))] 

        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.ActionResult, roType, "", true), result.Content.Headers.ContentType)
        assertTransactionalCache  result 
        compareObject expected parsedResult

let PersistWithValueTransientObject(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithValue"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
    
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
     
        let oType = ttc "RestfulObjects.Test.Data.WithValue"
        let oid = oType + "/" + ktc "1"
        let disabledValue = TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable")) :: (makeObjectPropertyMember "ADisabledValue" oid "A Disabled Value" (TObjectVal(103)))

        let arguments = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("AChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))])); 
                                                                            TProperty("AConditionalChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));                                                                             
                                                                            TProperty("ADateTimeValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AStringValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))

        let expected = [ TProperty(JsonPropertyNames.DomainType, TObjectVal(oType));
                         TProperty(JsonPropertyNames.InstanceId, TObjectVal(ktc "1"));
                         TProperty(JsonPropertyNames.Title, TObjectVal("1"));
                         TProperty(JsonPropertyNames.Links, TArray( [ TObjectJson(makeGetLinkProp RelValues.Self (sprintf "objects/%s" oid)  RepresentationTypes.Object oType); 
                                                                      TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" oType)  RepresentationTypes.DomainType "");
                                                                      //TObjectJson(makeIconLink());
                                                                      TObjectJson(arguments :: makePutLinkProp RelValues.Update (sprintf "objects/%s" oid)  RepresentationTypes.Object oType) ]));
                         TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("AChoicesValue", TObjectJson(makeObjectPropertyMember "AChoicesValue" oid "A Choices Value" (TObjectVal(3))));
                                                                           TProperty("AConditionalChoicesValue", TObjectJson(makeObjectPropertyMember "AConditionalChoicesValue" oid "A Conditional Choices Value" (TObjectVal(3))));
                                                                           TProperty("ADateTimeValue", TObjectJson(makePropertyMemberDateTime "objects" "ADateTimeValue" oid "A Date Time Value" "A datetime value for testing" true (TObjectVal(DateTime.Parse("2012-02-10T00:00:00Z").ToUniversalTime())))); 
                                                                           TProperty("ADisabledValue", TObjectJson(disabledValue));
                                                                           TProperty("AStringValue", TObjectJson(makePropertyMemberString "objects" "AStringValue" oid "A String Value" "A string value for testing" true (TObjectVal("one hundred four")) []));                                                            
                                                                           TProperty("AUserDisabledValue", TObjectJson(TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Not authorized to edit")) ::makeObjectPropertyMember "AUserDisabledValue" oid "A User Disabled Value" (TObjectVal(0)))); 
                                                                           TProperty("AValue", TObjectJson(makeObjectPropertyMember "AValue" oid "A Value" (TObjectVal(102)))); 
                                                                           TProperty("Id", TObjectJson(makeObjectPropertyMember "Id" oid "Id" (TObjectVal(1)))) ]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(oType));
                                                                              TProperty(JsonPropertyNames.FriendlyName, TObjectVal("With Value"));
                                                                              TProperty(JsonPropertyNames.PluralName, TObjectVal("With Values"));
                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                              TProperty(JsonPropertyNames.PresentationHint, TObjectVal("class1 class2"));
                                                                              TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) ) ]

        Assert.AreEqual(HttpStatusCode.Created, persistResult.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.Object, oType), persistResult.Content.Headers.ContentType)
        Assert.AreEqual(true, persistResult.Headers.CacheControl.NoCache)
        Assert.AreEqual((sprintf "http://localhost/objects/%s" oid), persistResult.Headers.Location.ToString())
        compareObject expected parsedPersist

let PersistWithValueTransientObjectFormalOnly(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithValue"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        RestfulObjectsControllerBase.DomainModel <-  RestControlFlags.DomainModelType.Formal

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  

        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
     
        let oType = ttc "RestfulObjects.Test.Data.WithValue"
        let oid = oType + "/" + ktc "1"
        let disabledValue = TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable")) :: (makePropertyMemberFormal "objects" "ADisabledValue" oid (TObjectVal(103)) false)

        let arguments = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("AChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))])); 
                                                                            TProperty("AConditionalChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));                                                                             
                                                                            TProperty("ADateTimeValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AStringValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))

        let expected = [ //TProperty(JsonPropertyNames.DomainType, TObjectVal(oType));
                         TProperty(JsonPropertyNames.InstanceId, TObjectVal(ktc "1"));
                         TProperty(JsonPropertyNames.Title, TObjectVal("1"));
                         TProperty(JsonPropertyNames.Links, TArray( [ TObjectJson(makeLinkPropWithMethodAndTypes "GET" RelValues.Self (sprintf "objects/%s" oid)  RepresentationTypes.Object oType "" false);
                                                                      TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" oType)  RepresentationTypes.DomainType "");
                                                                      //TObjectJson(makeIconLink());
                                                                      TObjectJson(arguments :: makeLinkPropWithMethodAndTypes "PUT" RelValues.Update (sprintf "objects/%s" oid)  RepresentationTypes.Object oType "" false) ]));
                         TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("AChoicesValue", TObjectJson(makePropertyMemberFormal "objects" "AChoicesValue" oid  (TObjectVal(3)) false));
                                                                           TProperty("AConditionalChoicesValue", TObjectJson(makePropertyMemberFormal "objects" "AConditionalChoicesValue" oid  (TObjectVal(3)) false));
                                                                           TProperty("ADateTimeValue", TObjectJson(makePropertyMemberFormal "objects" "ADateTimeValue" oid  (TObjectVal(DateTime.Parse("2012-02-10T00:00:00Z").ToUniversalTime())) true)); 
                                                                           TProperty("ADisabledValue", TObjectJson(disabledValue));
                                                                           TProperty("AStringValue", TObjectJson(makePropertyMemberFormal "objects" "AStringValue" oid (TObjectVal("one hundred four")) true));                                                            
                                                                           TProperty("AUserDisabledValue", TObjectJson(TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Not authorized to edit")) ::makePropertyMemberFormal "objects" "AUserDisabledValue" oid (TObjectVal(0)) false)); 
                                                                           TProperty("AValue", TObjectJson(makePropertyMemberFormal "objects" "AValue" oid (TObjectVal(102)) false)); 
                                                                           TProperty("Id", TObjectJson(makePropertyMemberFormal "objects" "Id" oid (TObjectVal(1)) false)) ]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([]) ) ]

        Assert.AreEqual(HttpStatusCode.Created, persistResult.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.Object, oType, "", false), persistResult.Content.Headers.ContentType)
        Assert.AreEqual(true, persistResult.Headers.CacheControl.NoCache)
        Assert.AreEqual((sprintf "http://localhost/objects/%s" oid), persistResult.Headers.Location.ToString())
        compareObject expected parsedPersist


let PersistWithReferenceTransientObject(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithReference"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
   
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
     
        let oType = ttc "RestfulObjects.Test.Data.WithReference"
        let roType = ttc "RestfulObjects.Test.Data.MostSimple"
        let roid = roType + "/" + ktc "1"
        //let disabledValue = TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable")) :: (makeObjectPropertyMember "ADisabledValue" oid "A Disabled Value" (TObjectVal("103")))
        let oid = oType + "/" + ktc "1"
        let pid = "AnEagerReference"
        let ourl = sprintf "objects/%s"  oid
        let purl = sprintf "%s/properties/%s" ourl pid



        let args1 = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))
        let msObj    = [ TProperty(JsonPropertyNames.DomainType, TObjectVal(roType)); TProperty(JsonPropertyNames.InstanceId, TObjectVal(ktc "1"));
                         TProperty(JsonPropertyNames.Title, TObjectVal("1"));
                         TProperty(JsonPropertyNames.Links, TArray([ TObjectJson(makeGetLinkProp RelValues.Self  (sprintf "objects/%s" roid)  RepresentationTypes.Object roType);
                                                                     TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" roType)  RepresentationTypes.DomainType "");
                                                                     //TObjectJson(makeIconLink()); 
                                                                     TObjectJson(args1 :: makePutLinkProp RelValues.Update  (sprintf "objects/%s" roid)  RepresentationTypes.Object roType) ]));
                         TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("Id", TObjectJson(makeObjectPropertyMember "Id" roid  "Id" (TObjectVal(1)))) ]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(roType));
                                                                              TProperty(JsonPropertyNames.FriendlyName, TObjectVal("Most Simple"));
                                                                              TProperty(JsonPropertyNames.PluralName, TObjectVal("Most Simples"));
                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                              TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) ) ]



        let valueRel1 = RelValues.Value + makeParm RelParamValues.Property "AChoicesReference"
        let valueRel6 = RelValues.Value + makeParm RelParamValues.Property "AConditionalChoicesReference"
        let valueRel2 = RelValues.Value + makeParm RelParamValues.Property "ADisabledReference"
        let valueRel3 = RelValues.Value + makeParm RelParamValues.Property "AReference"
        let valueRel4 = RelValues.Value + makeParm RelParamValues.Property "AnEagerReference"
        let valueRel5 = RelValues.Value + makeParm RelParamValues.Property "AnAutoCompleteReference"

        let val1 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel1 (sprintf "objects/%s/%s" roType (ktc "1"))  RepresentationTypes.Object roType)
        let val2 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel2 (sprintf "objects/%s/%s" roType (ktc "1"))  RepresentationTypes.Object roType)
        let val3 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel3 (sprintf "objects/%s/%s" roType (ktc "1"))  RepresentationTypes.Object roType)
        let val4 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeLinkPropWithMethodValue "GET" valueRel4 (sprintf "objects/%s/%s" roType (ktc "1"))  RepresentationTypes.Object roType (TObjectJson(msObj)) )
        let val5 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel5 (sprintf "objects/%s/%s" roType (ktc "1"))  RepresentationTypes.Object roType)
        let val6 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel6 (sprintf "objects/%s/%s" roType (ktc "1"))  RepresentationTypes.Object roType)


        let modifyRel = RelValues.Modify + makeParm RelParamValues.Property "AnEagerReference"

        let details = [ TProperty(JsonPropertyNames.Id, TObjectVal("AnEagerReference"));
                         TProperty(JsonPropertyNames.Value, val4);
                         TProperty(JsonPropertyNames.HasChoices, TObjectVal(false));
                         TProperty(JsonPropertyNames.Links, TArray( [ TObjectJson(makeGetLinkProp RelValues.Up ourl  RepresentationTypes.Object oType);
                                                                      TObjectJson(makeGetLinkProp RelValues.Self purl RepresentationTypes.ObjectProperty ""); 
                                                                      TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/properties/%s" oType pid)  RepresentationTypes.PropertyDescription "");
                                                       
                                                                      TObjectJson(TProperty(JsonPropertyNames.Arguments, TObjectJson( [TProperty(JsonPropertyNames.Value, TObjectVal(null))])) :: makePutLinkProp modifyRel purl RepresentationTypes.ObjectProperty "")]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.FriendlyName, TObjectVal("An Eager Reference"));
                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                              TProperty(JsonPropertyNames.ReturnType, TObjectVal(roType));
                                                                              TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));
                                                                              TProperty(JsonPropertyNames.Optional, TObjectVal(false))]) )]




        let arguments = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("AChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))])); 
                                                                            TProperty("AConditionalChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))])); 
                                                                            TProperty("ANullReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("AnAutoCompleteReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))])); 
                                                                            TProperty("AnEagerReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]));
                                                                            TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))

        let expected = [ TProperty(JsonPropertyNames.DomainType, TObjectVal(oType));
                         TProperty(JsonPropertyNames.InstanceId, TObjectVal(ktc "1"));
                         TProperty(JsonPropertyNames.Title, TObjectVal("1"));
                         TProperty(JsonPropertyNames.Links, TArray([ TObjectJson(makeGetLinkProp RelValues.Self (sprintf "objects/%s" oid)  RepresentationTypes.Object oType);
                                                                     TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" oType)  RepresentationTypes.DomainType "");
                                                                     //TObjectJson(makeIconLink());
                                                                     TObjectJson(arguments :: makePutLinkProp RelValues.Update (sprintf "objects/%s" oid)  RepresentationTypes.Object oType) ]));
                         TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("AChoicesReference", TObjectJson(makePropertyMemberShort "objects" "AChoicesReference" oid "A Choices Reference" "" roType false val1 []));
                                                                           TProperty("AConditionalChoicesReference", TObjectJson(makePropertyMemberShort "objects" "AConditionalChoicesReference" oid "A Conditional Choices Reference" "" roType false val6 []));
                                                                           TProperty("ADisabledReference", TObjectJson(TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable")) :: (makePropertyMemberShort "objects" "ADisabledReference" oid "A Disabled Reference" "" roType false val2 [])));
                                                                           TProperty("ANullReference", TObjectJson(makePropertyMemberShort "objects" "ANullReference" oid "A Null Reference" "" roType true (TObjectVal(null)) []));
                                                                           TProperty("AReference", TObjectJson(makePropertyMemberShort "objects" "AReference" oid "A Reference" "" roType false val3 []));
                                                                           TProperty("AnAutoCompleteReference", TObjectJson(makePropertyMemberShort "objects" "AnAutoCompleteReference" oid "An Auto Complete Reference" "" roType false val5 []));
                                                                           TProperty("AnEagerReference", TObjectJson(makePropertyMemberShort "objects" "AnEagerReference" oid "An Eager Reference" "" roType false val4 details));
                                                                           TProperty("Id", TObjectJson(makeObjectPropertyMember "Id" oid  "Id" (TObjectVal(1)))) ]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(oType));
                                                                              TProperty(JsonPropertyNames.FriendlyName, TObjectVal("With Reference"));
                                                                              TProperty(JsonPropertyNames.PluralName, TObjectVal("With References"));
                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                              TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) )]

        Assert.AreEqual(HttpStatusCode.Created, persistResult.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.Object, oType), persistResult.Content.Headers.ContentType)
        Assert.AreEqual(true, persistResult.Headers.CacheControl.NoCache)
        Assert.AreEqual((sprintf "http://localhost/objects/%s" oid), persistResult.Headers.Location.ToString())
        compareObject expected parsedPersist

let PersistWithCollectionTransientObject(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithCollection"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
  
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
       

        let oType = ttc "RestfulObjects.Test.Data.WithCollection"
        let oid = oType + "/" + ktc "1"
        let pid = "AnEagerCollection"
        let ourl = sprintf "objects/%s"  oid
        let purl = sprintf "%s/collections/%s" ourl pid

        let disabledValue = TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable")) :: (makeObjectPropertyMember "ADisabledValue" oid "A Disabled Value" (TObjectVal("103")))

        let valueRel = RelValues.Value + makeParm RelParamValues.Property "ACollection"
        let roType = ttc "RestfulObjects.Test.Data.MostSimple"

        let val1 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp JsonPropertyNames.Value (sprintf "objects/%s/%s" roType (ktc "1"))  RepresentationTypes.Object roType)

        let moid1 = roType + "/" + ktc "1"
        let moid2 = roType + "/" + ktc "2"

        let valueRel = RelValues.Value + makeParm RelParamValues.Collection "AnEagerCollection"

        let val3 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("1")) :: makeGetLinkProp valueRel (sprintf "objects/%s" moid1)  RepresentationTypes.Object roType)
        let val4 =  TObjectJson(TProperty(JsonPropertyNames.Title, TObjectVal("2")) :: makeGetLinkProp valueRel (sprintf "objects/%s" moid2)  RepresentationTypes.Object roType)
 
  

        let details = [ TProperty(JsonPropertyNames.Id, TObjectVal("AnEagerCollection"));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.ReturnType, TObjectVal(ResultTypes.List));
                                                                              TProperty(JsonPropertyNames.FriendlyName, TObjectVal("An Eager Collection"));
                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                              TProperty(JsonPropertyNames.PluralName, TObjectVal("Most Simples"));
                                                                              TProperty(JsonPropertyNames.MemberOrder, TObjectVal(0));
                                                                              TProperty(JsonPropertyNames.ElementType, TObjectVal(roType))]) );
                         TProperty(JsonPropertyNames.DisabledReason, TObjectVal("Field not editable"));
                         TProperty(JsonPropertyNames.Value,TArray( [ val3;val4 ] ));
                         TProperty(JsonPropertyNames.Links, TArray([ TObjectJson(makeGetLinkProp RelValues.Up ourl  RepresentationTypes.Object (oType));
                                                                     TObjectJson(makeLinkPropWithMethodAndTypes "GET" RelValues.Self purl RepresentationTypes.ObjectCollection "" roType true); 
                                                                     TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s/collections/%s" oType pid)  RepresentationTypes.CollectionDescription "")]))]                                                    







        let arguments = TProperty(JsonPropertyNames.Arguments, TObjectJson([TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null))]))]))

        let expected = [ TProperty(JsonPropertyNames.DomainType, TObjectVal(oType)); 
                         TProperty(JsonPropertyNames.InstanceId, TObjectVal(ktc "1"));
                         TProperty(JsonPropertyNames.Title, TObjectVal("1"));
                         TProperty(JsonPropertyNames.Links, TArray([ TObjectJson(makeGetLinkProp RelValues.Self (sprintf "objects/%s" oid)  RepresentationTypes.Object oType);
                                                                     TObjectJson(makeGetLinkProp RelValues.DescribedBy (sprintf "domain-types/%s" oType)  RepresentationTypes.DomainType "");
                                                                     //TObjectJson(makeIconLink());
                                                                     TObjectJson(arguments :: makePutLinkProp RelValues.Update (sprintf "objects/%s" oid)  RepresentationTypes.Object oType) ]));
                         TProperty(JsonPropertyNames.Members, TObjectJson([TProperty("ACollection", TObjectJson(makeCollectionMember  "ACollection" oid "A Collection" "" ResultTypes.List 2));
                                                                           TProperty("ACollectionViewModels", TObjectJson(makeCollectionMemberType  "ACollectionViewModels" oid "A Collection View Models" "" ResultTypes.List 2 (ttc "RestfulObjects.Test.Data.MostSimpleViewModel") "Most Simple View Models" ));
                                                                           TProperty("ADisabledCollection", TObjectJson( (makeCollectionMember  "ADisabledCollection" oid "A Disabled Collection" "" ResultTypes.List 2 )));
                                                                           TProperty("AnEmptyCollection", TObjectJson(makeCollectionMember  "AnEmptyCollection" oid "An Empty Collection" "an empty collection for testing" ResultTypes.List 0));
                                                                           TProperty("AnEagerCollection", TObjectJson(makeCollectionMemberTypeValue  "AnEagerCollection" oid "An Eager Collection" "" "list"  2 roType "Most Simples" (TArray([val3; val4 ])) details));                                                                         
                                                                           TProperty("ASet", TObjectJson(makeCollectionMember  "ASet" oid "A Set" "" "set" 2));
                                                                           TProperty("AnEmptySet", TObjectJson(makeCollectionMember  "AnEmptySet" oid "An Empty Set" "an empty set for testing" "set" 0));                                                                           
                                                                           TProperty("Id", TObjectJson(makeObjectPropertyMember "Id" oid  "Id" (TObjectVal(1)))) ]));
                         TProperty(JsonPropertyNames.Extensions, TObjectJson([TProperty(JsonPropertyNames.DomainType, TObjectVal(oType));
                                                                              TProperty(JsonPropertyNames.FriendlyName, TObjectVal("With Collection"));
                                                                              TProperty(JsonPropertyNames.PluralName, TObjectVal("With Collections"));
                                                                              TProperty(JsonPropertyNames.Description, TObjectVal(""));
                                                                              TProperty(JsonPropertyNames.IsService, TObjectVal(false))]) )]

        Assert.AreEqual(HttpStatusCode.Created, persistResult.StatusCode)
        Assert.AreEqual(new typeType(RepresentationTypes.Object, oType), persistResult.Content.Headers.ContentType)
        Assert.AreEqual(true, persistResult.Headers.CacheControl.NoCache)
        Assert.AreEqual((sprintf "http://localhost/objects/%s" oid), persistResult.Headers.Location.ToString())
        compareObject expected parsedPersist

let PersistWithValueTransientObjectValidateOnly(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithValue"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let voProp = new JProperty("x-ro-validate-only", true)          
        args.First.Last.AddAfterSelf(voProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
   
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonResult = readSnapshotToJson persistResult
         
        Assert.AreEqual(HttpStatusCode.NoContent, persistResult.StatusCode) 
        Assert.AreEqual("", jsonResult)

let PersistWithReferenceTransientObjectValidateOnly(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithReference"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let voProp = new JProperty("x-ro-validate-only", true)          
        args.First.Last.AddAfterSelf(voProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
  
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonResult = readSnapshotToJson persistResult
                 
        Assert.AreEqual(HttpStatusCode.NoContent, persistResult.StatusCode)     
        Assert.AreEqual("", jsonResult)

let PersistWithCollectionTransientObjectValidateOnly(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithCollection"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let voProp = new JProperty("x-ro-validate-only", true)          
        args.First.Last.AddAfterSelf(voProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
    
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonResult = readSnapshotToJson persistResult
            
        Assert.AreEqual(HttpStatusCode.NoContent, persistResult.StatusCode)
        Assert.AreEqual("", jsonResult)

let PersistWithValueTransientObjectValidateOnlyFail(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithValue"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let choiceValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "AChoicesValue")
        let m = choiceValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (choiceValue.First :?> JObject).Add(JsonPropertyNames.Value, null)

        let voProp = new JProperty("x-ro-validate-only", true)          
        args.First.Last.AddAfterSelf(voProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
   
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
        
        let oType = ttc "RestfulObjects.Test.Data.WithValue"
        let error = "Mandatory"

        let members = [TProperty("AChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null));
                                                               TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))])); 
                       TProperty("AConditionalChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(3))])); 
                       TProperty("ADateTimeValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(DateTime.Parse("2012-02-10T00:00:00Z").ToUniversalTime()))]));
                       TProperty("ADisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(103)) ]));
                       TProperty("AHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(105)) ]));
                       TProperty("AStringValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal("one hundred four")) ]));                       
                       TProperty("AUserDisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));
                       TProperty("AUserHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));
                       TProperty("AValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(102)) ]));                   
                       TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value,TObjectVal(0))]))]
    
        let expected = [ TProperty(JsonPropertyNames.Links, TArray([ TObjectJson([ TProperty(JsonPropertyNames.Rel, TObjectVal("describedby")); 
                                                                                   TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(sprintf "domain-types/%s" oType)))])  ]));                       
                         TProperty(JsonPropertyNames.DomainType, TObjectVal(oType))
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist

let PersistWithValueTransientObjectValidateOnlySimpleOnlyFail(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithValue"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let simpleProp = new JProperty("x-ro-domain-model", "simple")
        let parms =  new JObject (simpleProp)

        let args = CreateArgMap parms

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) (parms.ToString())
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[0]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let choiceValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "AChoicesValue")
        let m = choiceValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (choiceValue.First :?> JObject).Add(JsonPropertyNames.Value, null)

        let voProp = new JProperty("x-ro-validate-only", true)          
        args.First.Last.AddAfterSelf(voProp)
        args.First.Last.AddAfterSelf(simpleProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
    
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
        
        let oType = ttc "RestfulObjects.Test.Data.WithValue"
        let error = "Mandatory"

        let members = [TProperty("AChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null));
                                                               TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))])); 
                       TProperty("AConditionalChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(3))])); 
                       TProperty("ADateTimeValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(DateTime.Parse("2012-02-10T00:00:00Z").ToUniversalTime()))]));
                       TProperty("ADisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(103)) ]));
                       TProperty("AHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(105)) ]));
                       TProperty("AStringValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal("one hundred four")) ]));                       
                       TProperty("AUserDisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));
                       TProperty("AUserHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));
                       TProperty("AValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(102)) ]));                   
                       TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value,TObjectVal(0))]))]
    
        let expected = [ TProperty(JsonPropertyNames.DomainType, TObjectVal(oType))
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist

let PersistWithValueTransientObjectValidateOnlyFormalOnlyFail(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithValue"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let formalProp = new JProperty("x-ro-domain-model", "formal")
        let parms =  new JObject (formalProp)

        let args = CreateArgMap parms

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) (parms.ToString())
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let choiceValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "AChoicesValue")
        let m = choiceValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (choiceValue.First :?> JObject).Add(JsonPropertyNames.Value, null)

        let voProp = new JProperty("x-ro-validate-only", true)          
        args.First.Last.AddAfterSelf(voProp)
        args.First.Last.AddAfterSelf(formalProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
     

        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
        
        let oType = ttc "RestfulObjects.Test.Data.WithValue"
        let error = "Mandatory"

        let members = [TProperty("AChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null));
                                                               TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))])); 
                       TProperty("AConditionalChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(3))])); 
                       TProperty("ADateTimeValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(DateTime.Parse("2012-02-10T00:00:00Z").ToUniversalTime()))]));
                       TProperty("ADisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(103)) ]));
                       TProperty("AHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(105)) ]));
                       TProperty("AStringValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal("one hundred four")) ]));                       
                       TProperty("AUserDisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));
                       TProperty("AUserHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));
                       TProperty("AValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(102)) ]));                   
                       TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value,TObjectVal(0))]))]
    
        let expected = [ TProperty(JsonPropertyNames.Links, TArray([ TObjectJson([ TProperty(JsonPropertyNames.Rel, TObjectVal("describedby")); 
                                                                                   TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(sprintf "domain-types/%s" oType)))])  ]));
                      
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist




let PersistWithReferenceTransientObjectValidateOnlyFail(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithReference"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let choiceValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "AChoicesReference")
        let m = choiceValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (choiceValue.First :?> JObject).Add(JsonPropertyNames.Value, null)

        let voProp = new JProperty("x-ro-validate-only", true)          
        args.First.Last.AddAfterSelf(voProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
   
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
        
        let oType = ttc "RestfulObjects.Test.Data.WithReference"
        let oid = sprintf "objects/%s/%s" (ttc "RestfulObjects.Test.Data.MostSimple") (ktc "1")
        let error = "Mandatory"

        let valueRel1 = RelValues.Value + makeParm RelParamValues.Property "ADisabledReference"
        let valueRel2 = RelValues.Value + makeParm RelParamValues.Property "AReference"
        let valueRel3 = RelValues.Value + makeParm RelParamValues.Property "AChoicesReference"

        let members = [TProperty("AChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null));
                                                                   TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))]));                       
                       TProperty("AConditionalChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref  oid )) ]));      
                       TProperty("ADisabledReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref  oid  ))]));
                       TProperty("AHiddenReference", TObjectJson([TProperty(JsonPropertyNames.Value,  TObjectVal(null)) ]));               
                       TProperty("ANullReference", TObjectJson([TProperty(JsonPropertyNames.Value,  TObjectVal(null)) ]));   
                       TProperty("AReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref  oid )) ])); 
                       TProperty("AnAutoCompleteReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref  oid )) ]));     
                       TProperty("AnEagerReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref  oid )) ]));                     
                       TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value,TObjectVal(0))]))]
    
        let expected = [ TProperty(JsonPropertyNames.Links, TArray([ TObjectJson([ TProperty(JsonPropertyNames.Rel, TObjectVal("describedby")); 
                                                                                   TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(sprintf "domain-types/%s" oType)))])  ]));
                         TProperty(JsonPropertyNames.DomainType, TObjectVal(oType))
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist

let PersistWithCollectionTransientObjectValidateOnlyFail(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithCollection"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)
        let idValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "Id")
        let m = idValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (idValue.First :?> JObject).Add(JsonPropertyNames.Value, null)

        let voProp = new JProperty("x-ro-validate-only", true)          
        args.First.Last.AddAfterSelf(voProp)

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
   
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()

        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
        
        let oType = ttc "RestfulObjects.Test.Data.WithCollection"
        let oid = sprintf "objects/%s-1" (ttc "RestfulObjects.Test.Data.MostSimple")
        let error = "Mandatory"

        let members = [TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null));
                                                    TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))]))  ]
    
        let expected = [ TProperty(JsonPropertyNames.Links, TArray([ TObjectJson([ TProperty(JsonPropertyNames.Rel, TObjectVal("describedby")); 
                                                                                   TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(sprintf "domain-types/%s" oType)))])  ]));
                         TProperty(JsonPropertyNames.DomainType, TObjectVal(oType))
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist

let PersistWithValueTransientObjectFail(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithValue"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let choiceValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "AChoicesValue")
        let m = choiceValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (choiceValue.First :?> JObject).Add(JsonPropertyNames.Value, null)
       
        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
        
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()
        
        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
        
        let oType = ttc "RestfulObjects.Test.Data.WithValue"
        let error = "Mandatory"

        let members = [TProperty("AChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null));
                                                               TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))])); 
                       TProperty("AConditionalChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(3))])); 
                       TProperty("ADateTimeValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(DateTime.Parse("2012-02-10T00:00:00Z").ToUniversalTime()))]));
                       TProperty("ADisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(103)) ]));
                       TProperty("AHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(105)) ]));
                       TProperty("AStringValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal("one hundred four")) ]));

                       TProperty("AUserDisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));
                       TProperty("AUserHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));

                       TProperty("AValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(102)) ]));                   
                       TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value,TObjectVal(0))]))]
    
        let expected = [ TProperty(JsonPropertyNames.Links, TArray([ TObjectJson([ TProperty(JsonPropertyNames.Rel, TObjectVal("describedby")); 
                                                                                   TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(sprintf "domain-types/%s" oType)))])  ]));
                         TProperty(JsonPropertyNames.DomainType, TObjectVal(oType))
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist

let PersistWithValueTransientObjectFailInvalid(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithValue"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let choiceValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "AChoicesValue")
        let m = choiceValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (choiceValue.First :?> JObject).Add(JsonPropertyNames.Value, JValue("invalid"))
       
        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
     
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()
        
        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
        
        let oType = ttc "RestfulObjects.Test.Data.WithValue"
        let error = "cannot format value invalid as Int32"

        let members = [TProperty("AChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal("invalid"));
                                                               TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))])); 
                       TProperty("AConditionalChoicesValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(3))])); 
                       TProperty("ADateTimeValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(DateTime.Parse("2012-02-10T00:00:00Z").ToUniversalTime()))]));
                       TProperty("ADisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(103)) ]));
                       TProperty("AHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(105)) ]));
                       TProperty("AStringValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal("one hundred four")) ]));                       
                       TProperty("AUserDisabledValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));
                       TProperty("AUserHiddenValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(0)) ]));
                       TProperty("AValue", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(102)) ]));                   
                       TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value,TObjectVal(0))]))]
    
        let expected = [ TProperty(JsonPropertyNames.Links, TArray([ TObjectJson([ TProperty(JsonPropertyNames.Rel, TObjectVal("describedby")); 
                                                                                   TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(sprintf "domain-types/%s" oType)))])  ]));
                         TProperty(JsonPropertyNames.DomainType, TObjectVal(oType))
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist


let PersistWithReferenceTransientObjectFail(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithReference"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let choiceValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "AChoicesReference")
        let m = choiceValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (choiceValue.First :?> JObject).Add(JsonPropertyNames.Value, null)
     
        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
       
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()
        
        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)

        let roType = ttc "RestfulObjects.Test.Data.MostSimple"
        let oType = ttc "RestfulObjects.Test.Data.WithReference"
        let oid = sprintf "objects/%s-1" roType
        let error = "Mandatory"

        let valueRel1 = RelValues.Value + makeParm RelParamValues.Collection "ADisabledReference"
        let valueRel2 = RelValues.Value + makeParm RelParamValues.Collection "AReference"
        

        let members = [TProperty("AChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null));
                                                                   TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))]));
                       TProperty("AConditionalChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value,TObjectJson( makeHref (sprintf "objects/%s/%s" roType (ktc "1"))))]));                              
                       TProperty("ADisabledReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref (sprintf "objects/%s/%s" roType (ktc "1"))))]));
                       TProperty("AHiddenReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null)) ]));               
                       TProperty("ANullReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null)) ])); 
                       TProperty("AReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref (sprintf "objects/%s/%s" roType (ktc "1")))) ]));  
                       TProperty("AnAutoCompleteReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref (sprintf "objects/%s/%s" roType (ktc "1")))) ]));                                    
                       TProperty("AnEagerReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref (sprintf "objects/%s/%s" roType (ktc "1")))) ]));                                   
                       TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value,TObjectVal(0))]))]
    
        let expected = [ TProperty(JsonPropertyNames.Links, TArray([ TObjectJson([ TProperty(JsonPropertyNames.Rel, TObjectVal("describedby")); 
                                                                                   TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(sprintf "domain-types/%s" oType)))])  ]));
                         TProperty(JsonPropertyNames.DomainType, TObjectVal(oType))
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist

let PersistWithReferenceTransientObjectFailInvalid(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithReference"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
        
        let eoid = sprintf "objects/%s/%s" (ttc "RestfulObjects.Test.Data.WithValue") (ktc "1")    
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)

        let choiceValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "AChoicesReference")
        let m = choiceValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (choiceValue.First :?> JObject).Add(JsonPropertyNames.Value, new JObject( new JProperty(JsonPropertyNames.Href, new JValue( (new hrefType(eoid)).ToString() ))))
     
        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
      
        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()
        
        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
        
        let oType = ttc "RestfulObjects.Test.Data.WithReference"
        let oid = sprintf "objects/%s/%s" (ttc "RestfulObjects.Test.Data.MostSimple") (ktc "1")
        let error = "Not a suitable type; must be a Most Simple"

        let valueRel1 = RelValues.Value + makeParm RelParamValues.Property "ADisabledReference"
        let valueRel2 = RelValues.Value + makeParm RelParamValues.Property "AReference"
        let valueRel3 = RelValues.Value + makeParm RelParamValues.Property "AChoicesReference"

        let members = [TProperty("AChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref eoid));
                                                                   TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))]));                       
                       TProperty("AConditionalChoicesReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref oid))]));   
                       TProperty("ADisabledReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref  oid  ))]));
                       TProperty("AHiddenReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null)) ]));               
                       TProperty("ANullReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null)) ]));       
                       TProperty("AReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref  oid  )) ])); 
                       TProperty("AnAutoCompleteReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref  oid  )) ]));     
                       TProperty("AnEagerReference", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectJson( makeHref  oid  )) ]));                  
                       TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value,TObjectVal(0))]))]
    
        let expected = [ TProperty(JsonPropertyNames.Links, TArray([ TObjectJson([ TProperty(JsonPropertyNames.Rel, TObjectVal("describedby")); 
                                                                                   TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(sprintf "domain-types/%s" oType)))])  ]));
                         TProperty(JsonPropertyNames.DomainType, TObjectVal(oType))
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist


let PersistWithCollectionTransientObjectFail(api : RestfulObjectsControllerBase) = 
        let oType = ttc "RestfulObjects.Test.Data.RestDataRepository"
        let pid = "CreateTransientWithCollection"
        let ourl = sprintf "%s/%s" "services"  oType
        let purl = sprintf "%s/actions/%s/invoke" ourl pid

        let args = CreateArgMap (new JObject())

        api.Request <- jsonPostMsg (sprintf "http://localhost/%s" purl) ""
        let transientResult = api.PostInvokeOnService(oType, pid, args)
        let jsonTransient = readSnapshotToJson transientResult
        let parsedTransient = JObject.Parse(jsonTransient)
             
        let result = parsedTransient |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Result)
        let links = result.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Links)
        let args = links.First.[1]  |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Arguments)
        let idValue = (args.First |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Members)).First  |> Seq.find  (fun i -> (i :?> JProperty).Name = "Id")
        let m = idValue.First |>  Seq.find  (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Value)
        m.Remove()
        (idValue.First :?> JObject).Add(JsonPropertyNames.Value, null)       

        let pArgs = CreatePersistArgMap (args.First :?> JObject)  
        

        let href =  links.First.[0] |> Seq.find (fun i -> (i :?> JProperty).Name = JsonPropertyNames.Href)
        let link =  (href :?> JProperty).Value.ToString()
        let dt = link.Split('/').Last()
        
        api.Request <- jsonPostMsg link (args.First.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let parsedPersist = JObject.Parse(jsonPersist)
        
        let oType = ttc "RestfulObjects.Test.Data.WithCollection"
        let oid = sprintf "objects/%s-1" (ttc "RestfulObjects.Test.Data.MostSimple")
        let error = "Mandatory"

        let members = [TProperty("Id", TObjectJson([TProperty(JsonPropertyNames.Value, TObjectVal(null));
                                                    TProperty(JsonPropertyNames.InvalidReason, TObjectVal(error))]))  ]
    
        let expected = [ TProperty(JsonPropertyNames.Links, TArray([ TObjectJson([ TProperty(JsonPropertyNames.Rel, TObjectVal("describedby")); 
                                                                                   TProperty(JsonPropertyNames.Href, TObjectVal(new hrefType(sprintf "domain-types/%s" oType)))])  ]));
                         TProperty(JsonPropertyNames.DomainType, TObjectVal(oType))
                         TProperty(JsonPropertyNames.Members, TObjectJson(members))]

        Assert.AreEqual( unprocessableEntity, persistResult.StatusCode) 
        Assert.AreEqual(new typeType(RepresentationTypes.BadArguments), persistResult.Content.Headers.ContentType)
        compareObject expected parsedPersist

let PersistMostSimpleTransientObjectMissingArgs(api : RestfulObjectsControllerBase) = 
      
        let dt = ttc "RestfulObjects.Test.Data.MostSimple"
        let props =  new JObject ([new JProperty(JsonPropertyNames.DomainType, dt)])

        let pArgs = CreatePersistArgMap props

        api.Request <- jsonPostMsg "http://localhost/objects" ""
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonResult = readSnapshotToJson persistResult

        Assert.AreEqual(HttpStatusCode.BadRequest, persistResult.StatusCode)
        Assert.AreEqual("199 RestfulObjects \"Missing arguments\"", persistResult.Headers.Warning.ToString())
        Assert.AreEqual("", jsonResult)

let PersistMostSimpleTransientObjectMissingArgsValidateOnly(api : RestfulObjectsControllerBase) = 
      
        let dt = ttc "RestfulObjects.Test.Data.MostSimple"
        let props =  new JObject ([new JProperty(JsonPropertyNames.DomainType, dt); new JProperty("x-ro-validate-only", true)]) 

        let pArgs = CreatePersistArgMap props

        api.Request <- jsonPostMsg "http://localhost/objects" (props.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonResult = readSnapshotToJson persistResult

        Assert.AreEqual(HttpStatusCode.BadRequest, persistResult.StatusCode)
        Assert.AreEqual("199 RestfulObjects \"Missing arguments\"", persistResult.Headers.Warning.ToString())
        Assert.AreEqual("", jsonResult)

// new

let PersistMostSimpleTransientObjectMissingMemberArgs(api : RestfulObjectsControllerBase) = 
      
        let dt = ttc "RestfulObjects.Test.Data.MostSimple"

        let props =  new JObject ([new JProperty(JsonPropertyNames.DomainType, dt);
                                   new JProperty(JsonPropertyNames.Links, new JArray(new JObject([ new JProperty(JsonPropertyNames.Rel, "describedby"); 
                                                                                     new JProperty(JsonPropertyNames.Href, sprintf "http://domain-types/%s" dt   )])))])

        let pArgs = CreatePersistArgMap props
        
        api.Request <- jsonPostMsg "http://localhost/objects" (props.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonResult = readSnapshotToJson persistResult
       
        Assert.AreEqual(HttpStatusCode.BadRequest, persistResult.StatusCode)
        Assert.AreEqual("199 RestfulObjects \"Missing arguments\"", persistResult.Headers.Warning.ToString())
        Assert.AreEqual("", jsonResult)

let PersistMostSimpleTransientObjectNullDomainType(api : RestfulObjectsControllerBase) = 
      
        let dt = ttc "RestfulObjects.Test.Data.MostSimple"

        let props =  new JObject ([new JProperty(JsonPropertyNames.Members, new JObject(new JProperty("Id", new JObject(new JProperty(JsonPropertyNames.Value, 0)))));
                                   new JProperty("x-ro-domain-model", "simple");
                                   new JProperty(JsonPropertyNames.Links, new JArray(new JObject([ new JProperty(JsonPropertyNames.Rel, "describedby"); 
                                                                                     new JProperty(JsonPropertyNames.Href, sprintf "http://domain-types/%s" dt   )])))])


        let pArgs = CreatePersistArgMap props
        
        api.Request <- jsonPostMsg "http://localhost/objects" (props.ToString())
        let persistResult = api.PostPersist(null, pArgs)
        let jsonResult = readSnapshotToJson persistResult
      
        Assert.AreEqual(HttpStatusCode.BadRequest, persistResult.StatusCode)
        //Assert.AreEqual("199 RestfulObjects \"Missing arguments\"", persistResult.Headers.Warning.ToString())
        Assert.AreEqual("", jsonResult)

let PersistMostSimpleTransientObjectEmptyDomainType(api : RestfulObjectsControllerBase) = 
      
        let dt = ttc "RestfulObjects.Test.Data.MostSimple"

        let props =  new JObject ([new JProperty(JsonPropertyNames.Members, new JObject(new JProperty("Id", new JObject(new JProperty(JsonPropertyNames.Value, 0)))));
                                   new JProperty(JsonPropertyNames.DomainType, dt);
                                   new JProperty("x-ro-domain-model", "formal")])

        let pArgs = CreatePersistArgMap props
        
        api.Request <- jsonPostMsg "http://localhost/objects" (props.ToString())
        let persistResult = api.PostPersist("", pArgs)
        let jsonResult = readSnapshotToJson persistResult

        Assert.AreEqual(HttpStatusCode.BadRequest, persistResult.StatusCode)
        //Assert.AreEqual("199 RestfulObjects \"Missing arguments\"", persistResult.Headers.Warning.ToString())
        Assert.AreEqual("", jsonResult)


let PersistMostSimpleTransientObjectMalformedMemberArgs(api : RestfulObjectsControllerBase) = 
      
        let dt = ttc "RestfulObjects.Test.Data.MostSimple"

        let props =  new JObject ([new JProperty(JsonPropertyNames.Members, new JObject(new JProperty("malformed", 0)));
                                   new JProperty(JsonPropertyNames.DomainType, dt);
                                   new JProperty(JsonPropertyNames.Links, new JArray(new JObject([ new JProperty(JsonPropertyNames.Rel, "describedby"); 
                                                                                     new JProperty(JsonPropertyNames.Href, sprintf "http://domain-types/%s" dt   )])))])

        let pArgs = CreatePersistArgMap props
        
        api.Request <- jsonPostMsg "http://localhost/objects" (props.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonResult = readSnapshotToJson persistResult
  
        Assert.AreEqual(HttpStatusCode.BadRequest, persistResult.StatusCode)
        Assert.AreEqual("199 RestfulObjects \"Malformed arguments\"", persistResult.Headers.Warning.ToString())
        Assert.AreEqual("", jsonResult)

let PersistUnknownTypeTransientObject(api : RestfulObjectsControllerBase) = 
      
        let dt = ttc "RestfulObjects.Test.Data.NoSuchType"

        let props =  new JObject ([new JProperty(JsonPropertyNames.Members, new JObject(new JProperty("Id", new JObject(new JProperty(JsonPropertyNames.Value, 0)))));
                                   new JProperty(JsonPropertyNames.DomainType, dt);
                                   new JProperty(JsonPropertyNames.Links, new JArray(new JObject([ new JProperty(JsonPropertyNames.Rel, "describedby"); 
                                                                                     new JProperty(JsonPropertyNames.Href, sprintf "http://domain-types/%s" dt   )])))])

        let pArgs = CreatePersistArgMap props
        
        api.Request <- jsonPostMsg "http://localhost/objects" (props.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonResult = readSnapshotToJson persistResult
        
        Assert.AreEqual(HttpStatusCode.NotFound, persistResult.StatusCode)
        Assert.AreEqual(sprintf "199 RestfulObjects \"No such domain type %s\"" dt, persistResult.Headers.Warning.ToString())
        Assert.AreEqual("", jsonResult)

let PersistNoKeyTransientObject(api : RestfulObjectsControllerBase) = 
      
        let dt = ttc "RestfulObjects.Test.Data.NoKey"

        let props =  new JObject ([new JProperty(JsonPropertyNames.Members, new JObject(new JProperty("Name", new JObject(new JProperty(JsonPropertyNames.Value, "aName")))));
                                   new JProperty(JsonPropertyNames.DomainType, dt);
                                   new JProperty(JsonPropertyNames.Links, new JArray(new JObject([ new JProperty(JsonPropertyNames.Rel, "describedby"); 
                                                                                     new JProperty(JsonPropertyNames.Href, sprintf "http://domain-types/%s" dt   )])))])

        let pArgs = CreatePersistArgMap props
        
        api.Request <- jsonPostMsg "http://localhost/objects/" (props.ToString())
        let persistResult = api.PostPersist(dt, pArgs)
        let jsonPersist = readSnapshotToJson persistResult
        let jsonResult = JObject.Parse(jsonPersist)
        
// different stack trace on sercver - just test not empty body 
//        let expected = [TProperty(JsonPropertyNames.Message, TObjectVal("Cannot build URI : object key is null or empty"));
//                         TProperty(JsonPropertyNames.StackTrace, TArray([ TObjectVal( new errorType("   at RestfulObjects.Test.Data.WithError.AnError() in C:\Naked Objects Internal\REST\RestfulObjects.Test.Data\WithError.cs:line 12"));
//                                                                          TObjectVal( new errorType("   at RestfulObjects.Test.Data.WithError.AnError() in C:\Naked Objects Internal\REST\RestfulObjects.Test.Data\WithError.cs:line 12"));
//                                                                          TObjectVal( new errorType("   at RestfulObjects.Test.Data.WithError.AnError() in C:\Naked Objects Internal\REST\RestfulObjects.Test.Data\WithError.cs:line 12"));
//                                                                          TObjectVal( new errorType("   at RestfulObjects.Test.Data.WithError.AnError() in C:\Naked Objects Internal\REST\RestfulObjects.Test.Data\WithError.cs:line 12"));
//                                                                          TObjectVal( new errorType("   at RestfulObjects.Test.Data.WithError.AnError() in C:\Naked Objects Internal\REST\RestfulObjects.Test.Data\WithError.cs:line 12"));
//                                                                          TObjectVal( new errorType("   at RestfulObjects.Test.Data.WithError.AnError() in C:\Naked Objects Internal\REST\RestfulObjects.Test.Data\WithError.cs:line 12"));
//                                                                          TObjectVal( new errorType("   at RestfulObjects.Test.Data.WithError.AnError() in C:\Naked Objects Internal\REST\RestfulObjects.Test.Data\WithError.cs:line 12"));
//                                                                          TObjectVal( new errorType("   at RestfulObjects.Test.Data.WithError.AnError() in C:\Naked Objects Internal\REST\RestfulObjects.Test.Data\WithError.cs:line 12"))]));
//                        TProperty(JsonPropertyNames.Links, TArray([]))
//                        TProperty(JsonPropertyNames.Extensions, TObjectJson([]))]

        Assert.AreEqual(HttpStatusCode.InternalServerError, persistResult.StatusCode)
        Assert.AreEqual("199 RestfulObjects \"Cannot build URI : object key is null or empty\"", persistResult.Headers.Warning.ToString())//
//        compareObject expected jsonResult
        Assert.AreNotEqual("", jsonResult)