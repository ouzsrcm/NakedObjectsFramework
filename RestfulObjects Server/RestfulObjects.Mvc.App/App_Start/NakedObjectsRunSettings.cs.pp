﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Menu;
using NakedObjects.Architecture.Menu;
using NakedObjects.Core.Configuration;
using NakedObjects.Persistor.Entity.Configuration;

namespace $rootnamespace$ {
    public class NakedObjectsRunSettings {

	    public static string RestRoot {
            get { return ""; }
        }

		private static string[] Namespaces {
            get {
                return new string[] { };
            }
        }

		private static Type[] Services {
            get {
                return new Type[] {
                };
            }
        }
	    
		private static Type[] Types {
            get {
                return new Type[] {
                };
            }
        }

        public static ReflectorConfiguration ReflectorConfig() {
            return new ReflectorConfiguration(Types, Services, Namespaces, MainMenus);
        }

        public static EntityObjectStoreConfiguration EntityObjectStoreConfig() {
            var config = new EntityObjectStoreConfiguration();
            //e.g. config.UsingCodeFirstContext(() => new MyDbContext());
            return config;
        }

        public static IMenu[] MainMenus(IMenuFactory factory) {
            //e.g. var menu1 = factory.NewMenu<MyRepository>(true);
            return new IMenu[] { };
        }
    }
}