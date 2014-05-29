﻿// Copyright © Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Value;

namespace NakedObjects {
    [TestClass]
    public class FileAttachmentTest {
        [TestMethod]
        public void TestConstructors() {
            var resource = new byte[] {};

            var fa = new FileAttachment(resource);

            Assert.AreEqual(resource, fa.GetResourceAsByteArray());

            fa = new FileAttachment(resource, "MyName");

            Assert.AreEqual(resource, fa.GetResourceAsByteArray());
            Assert.AreEqual("MyName", fa.Name);

            fa = new FileAttachment(resource, "MyName2", "Word Doc");

            Assert.AreEqual(resource, fa.GetResourceAsByteArray());
            Assert.AreEqual("MyName2", fa.Name);
            Assert.AreEqual("Word Doc", fa.MimeType);
        }
    }
}