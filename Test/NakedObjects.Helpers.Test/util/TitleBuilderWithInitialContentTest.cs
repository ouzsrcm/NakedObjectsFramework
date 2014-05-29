// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NakedObjects {
    [TestClass]
    public class TitleBuilderWithInitialContentTest {
        private TitleBuilder builder;

        [TestInitialize]
        public void NewBuilder() {
            builder = new TitleBuilder("Text");
        }

        private void AssertTitleIs(string expected) {
            Assert.AreEqual(expected, builder.ToString());
        }

        [TestMethod]
        public void TestNewBuilderContainsUnmodifiedText() {
            builder = new TitleBuilder("Text");
            AssertTitleIs("Text");
        }


        [TestMethod]
        public void TestConcatAddsText() {
            builder.Concat("added");
            AssertTitleIs("Textadded");
        }

        [TestMethod]
        public void TestConcatAddsTextWithJoiner() {
            builder.Concat("+", "added");
            AssertTitleIs("Text+added");
        }

        [TestMethod]
        public void TestConcatAddsTitleAttribute() {
            builder.Concat(new ObjectWithTitleAttribute());
            AssertTitleIs("Textfrom property");
        }

        [TestMethod]
        public void TestAppendAddsTextAndSpace() {
            builder.Append("added");
            AssertTitleIs("Text added");
        }

        [TestMethod]
        public void TestAppendNullAddsNoTextAndNoSpace() {
            builder.Append(null);
            AssertTitleIs("Text");
        }

        [TestMethod]
        public void TestAppendAddsToString() {
            builder.Append(new ObjectWithToString());
            AssertTitleIs("Text from ToString");
        }

        [TestMethod]
        public void TestAppendAddsToStringWithJoiner() {
            builder.Append("-", new ObjectWithToString());
            AssertTitleIs("Text- from ToString");
        }


        [TestMethod]
        public void TestAppendAddsTitleMethod() {
            builder.Append(new ObjectWithTitleMethod());
            AssertTitleIs("Text from Title method");
        }

        [TestMethod]
        public void TestAppendAddsTitleAttribute() {
            builder.Append(new ObjectWithTitleAttribute());
            AssertTitleIs("Text from property");
        }


        [TestMethod]
        public void TestAppendAddsNullTitleAttribute() {
            builder.Append(new ObjectWithNullTitleAttribute());
            AssertTitleIs("Text");
        }

        [TestMethod]
        public void TestAppendAddsTitleAttributeThatIsAReference() {
            builder.Append(new ObjectWithTitleAttributeThatIsAReference());
            AssertTitleIs("Text from Title method");
        }


        [TestMethod]
        public void TestConcatNoJoiner() {
            builder.Concat(":", null, "d", null);
            AssertTitleIs("Text");
        }

        [TestMethod]
        public void TestAppendNoJoiner() {
            builder.Append(":", null, "d", null);
            AssertTitleIs("Text");
        }

        [TestMethod]
        public void TestAppendNoJoinerNoDefault() {
            builder.Append(":", null, "d", null);
            AssertTitleIs("Text");
        }

        [TestMethod]
        public void TestAppendFormatWithDefault() {
            builder.Append(":", null, "d", "no date");
            AssertTitleIs("Text: no date");
        }

        [TestMethod]
        public void TestTitleTruncated() {
            builder.Append("no date");
            builder.Truncate(3);
            AssertTitleIs("Text no date");
            builder.Truncate(2);
            AssertTitleIs("Text no ...");
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void TestTitleTruncateLenghtChecked() {
            builder.Truncate(0);
        }

        [TestMethod]
        public void TestAppendFormat() {
            builder.Append(":", new DateTime(2007, 4, 2), "d", null);
            AssertTitleIs("Text: 02/04/2007");
        }

        [TestMethod]
        public void Test() {
            builder.Append("added");
            AssertTitleIs("Text added");
        }
    }
}