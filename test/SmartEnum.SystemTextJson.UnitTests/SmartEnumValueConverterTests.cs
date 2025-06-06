namespace Ardalis.SmartEnum.SystemTextJson.UnitTests
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Xunit;

    public class SmartEnumValueConverterTests
    {
        public class TestClass
        {
            [JsonConverter(typeof(SmartEnumValueConverter<TestEnumBoolean, bool>))]
            public TestEnumBoolean Bool { get; set; }

            [JsonConverter(typeof(SmartEnumValueConverter<TestEnumByte, byte>))]
            public TestEnumByte Byte { get; set; }

            [JsonConverter(typeof(SmartEnumValueConverter<TestEnumSByte, sbyte>))]
            public TestEnumSByte SByte { get; set; }

            [JsonConverter(typeof(SmartEnumValueConverter<TestEnumInt16, short>))]
            public TestEnumInt16 Int16 { get; set; }

            [JsonConverter(typeof(SmartEnumValueConverter<TestEnumInt32, int>))]
            public TestEnumInt32 Int32 { get; set; }

            [JsonConverter(typeof(SmartEnumValueConverter<TestEnumDouble, double>))]
            public TestEnumDouble Double { get; set; }

            [JsonConverter(typeof(SmartEnumValueConverter<TestEnumString, string>))]
            public TestEnumString String { get; set; }

            public IDictionary<TestEnumInt32, string> DictInt32String { get; set; }

            public IDictionary<TestEnumString, string> DictStringString { get; set; }
        }

        static readonly TestClass TestInstance = new TestClass
        {
            Bool = TestEnumBoolean.Instance,
            Byte = TestEnumByte.Instance,
            SByte = TestEnumSByte.Instance,
            Int16 = TestEnumInt16.Instance,
            Int32 = TestEnumInt32.Instance,
            Double = TestEnumDouble.Instance,
            String = TestEnumString.Instance,
            DictInt32String = TestDictInt32EnumString.Instance,
            DictStringString = TestDictStringEnumString.Instance
        };

        static readonly string JsonString = JsonSerializer.Serialize(new
        {
            Bool = true,
            Byte = 1,
            SByte = 1,
            Int16 = 1,
            Int32 = 1,
            Double = 1.2,
            String = "1.5",
            DictInt32String = new DictInt32EnumStringJson(),
            DictStringString = new DictStringEnumStringJson()
        }, TestJsonConverters.ValueConverterOptions);

        [Fact]
        public void SerializesValue()
        {
            var json = JsonSerializer.Serialize(TestInstance, TestJsonConverters.ValueConverterOptions);

            json.Should().Be(JsonString);
        }

        [Fact]
        public void DeserializesValue()
        {
            var obj = JsonSerializer.Deserialize<TestClass>(JsonString, TestJsonConverters.ValueConverterOptions);

            obj.Bool.Should().BeSameAs(TestEnumBoolean.Instance);
            obj.Byte.Should().BeSameAs(TestEnumByte.Instance);
            obj.SByte.Should().BeSameAs(TestEnumSByte.Instance);
            obj.Int16.Should().BeSameAs(TestEnumInt16.Instance);
            obj.Int32.Should().BeSameAs(TestEnumInt32.Instance);
            obj.Double.Should().BeSameAs(TestEnumDouble.Instance);
            obj.String.Should().BeSameAs(TestEnumString.Instance);
            obj.DictInt32String.Should().BeEquivalentTo(TestDictInt32EnumString.Instance);
            obj.DictStringString.Should().BeEquivalentTo(TestDictStringEnumString.Instance);
        }

        [Fact]
        public void DeserializesNullByDefault()
        {
            string json = @"{}";

            var obj = JsonSerializer.Deserialize<TestClass>(json);

            obj.Bool.Should().BeNull();
            obj.Byte.Should().BeNull();
            obj.SByte.Should().BeNull();
            obj.Int16.Should().BeNull();
            obj.Int32.Should().BeNull();
            obj.Double.Should().BeNull();
            obj.String.Should().BeNull();
            obj.DictInt32String.Should().BeNull();
            obj.DictStringString.Should().BeNull();
        }

        [Fact]
        public void DeserializeThrowsWhenNotFound()
        {
            string json = @"{ ""Bool"": false }";

            Action act = () => JsonSerializer.Deserialize<TestClass>(json);

            act.Should()
                .Throw<JsonException>()
                .WithMessage($@"Error converting value 'False' to a smart enum.")
                .WithInnerException<SmartEnumNotFoundException>()
                .WithMessage($@"No {nameof(TestEnumBoolean)} with Value False found.");
        }

        public static TheoryData<string, string> NotValidData =>
            new TheoryData<string, string>
            {
                { @"{ ""Bool"": 1 }", @"Cannot get the value of a token type 'Number' as a boolean." },
                { @"{ ""Byte"": true }", @"Cannot get the value of a token type 'True' as a number." },
                { @"{ ""SByte"": true }", @"Cannot get the value of a token type 'True' as a number." },
                { @"{ ""Int16"": true }", @"Cannot get the value of a token type 'True' as a number." },
                { @"{ ""Int32"": true }", @"Cannot get the value of a token type 'True' as a number." },
                { @"{ ""Double"": true }", @"Cannot get the value of a token type 'True' as a number." },
                { @"{ ""String"": true }", @"Cannot get the value of a token type 'True' as a string." },
            };

        [Theory]
        [MemberData(nameof(NotValidData))]
        public void DeserializeThrowsWhenNotValid(string json, string message)
        {
            Action act = () => JsonSerializer.Deserialize<TestClass>(json);

            act.Should()
                .Throw<JsonException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage(message);
        }

        // JsonSerializer doesn't call the converter on null values
        //[Fact]
        //public void DeserializeThrowsWhenNull()
        //{
        //    string json = @"{ ""Bool"": null }";

        //    Action act = () => JsonSerializer.Deserialize<TestClass>(json);

        //    act.Should()
        //        .Throw<JsonException>()
        //        .WithMessage($@"Error converting Null to TestEnumBoolean.");
        //}   
    }
}