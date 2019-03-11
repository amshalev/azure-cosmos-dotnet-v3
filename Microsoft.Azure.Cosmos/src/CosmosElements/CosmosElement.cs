﻿//-----------------------------------------------------------------------
// <copyright file="CosmosElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Azure.Cosmos.CosmosElements
{
    using Microsoft.Azure.Cosmos.Query;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Azure.Cosmos.Json;

    internal abstract class CosmosElement
    {
        protected CosmosElement(CosmosElementType cosmosItemType)
        {
            this.Type = cosmosItemType;
        }

        public CosmosElementType Type
        {
            get;
        }

        public abstract void WriteTo(IJsonWriter jsonWriter);

        public object ToObject()
        {
            object obj;
            switch (this.Type)
            {
                case CosmosElementType.Array:
                    List<object> objectArray = new List<object>();
                    CosmosArray cosmosArray = this as CosmosArray;
                    foreach (CosmosElement arrayItem in cosmosArray)
                    {
                        objectArray.Add(arrayItem.ToObject());
                    }

                    obj = objectArray.ToArray();
                    break;

                case CosmosElementType.Boolean:
                    obj = (this as CosmosBoolean).Value;
                    break;

                case CosmosElementType.Null:
                    obj = null;
                    break;

                case CosmosElementType.Number:
                    CosmosNumber cosmosNumber = this as CosmosNumber;
                    if (cosmosNumber.IsFloatingPoint)
                    {
                        obj = cosmosNumber.AsFloatingPoint().Value;
                    }
                    else
                    {
                        obj = cosmosNumber.AsInteger().Value;
                    }

                    break;

                case CosmosElementType.Object:
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, CosmosElement> kvp in this as CosmosObject)
                    {
                        dictionary.Add(kvp.Key, kvp.Value.ToObject());
                    }

                    obj = dictionary;
                    break;

                case CosmosElementType.String:
                    obj = (this as CosmosString).Value;
                    break;

                default:
                    throw new ArgumentException($"Unknown {nameof(CosmosElementType)}: {this.Type}");
            }

            return obj;
        }

        public static CosmosElement FromObject(object obj)
        {
            CosmosElement cosmosElement;
            if (obj is null)
            {
                cosmosElement = CosmosNull.Create();
            }
            else if (obj is bool boolean)
            {
                cosmosElement = CosmosBoolean.Create(boolean);
            }
            else if (obj is double objAsDouble)
            {
                cosmosElement = CosmosNumber.Create(objAsDouble);
            }
            else if (obj is long objAsLong)
            {
                cosmosElement = CosmosNumber.Create(objAsLong);
            }
            else if (obj is string objAsString)
            {
                cosmosElement = CosmosString.Create(objAsString);
            }
            else if (obj is object[] objectArray)
            {
                List<CosmosElement> cosmosElements = new List<CosmosElement>();
                foreach (object item in objectArray)
                {
                    cosmosElements.Add(CosmosElement.FromObject(item));
                }

                cosmosElement = CosmosArray.Create(cosmosElements);
            }
            else if(obj is Dictionary<string, object> dictionary)
            {
                Dictionary<string, CosmosElement> cosmosDictionary = new Dictionary<string, CosmosElement>();
                foreach(KeyValuePair<string, object> kvp in dictionary)
                {
                    cosmosDictionary.Add(kvp.Key, CosmosElement.FromObject(kvp.Value));
                }

                cosmosElement = CosmosObject.Create(cosmosDictionary);
            }
            else
            {
                throw new ArgumentException($"Can't conver object {obj} to {nameof(CosmosElement)}");
            }

            return cosmosElement;
        }
    }
}
