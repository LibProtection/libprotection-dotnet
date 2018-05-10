using Newtonsoft.Json;
using System;

namespace LibProtection.Injections.Tests
{
    class TokenConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Token) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            var jobject = Newtonsoft.Json.Linq.JObject.Load(reader);

            var isTrivial = jobject.Value<bool>(nameof(Token.IsTrivial));

            var text = jobject.Value<string>(nameof(Token.Text));
            var languageProviderName = jobject.Value<string>(nameof(Token.LanguageProvider));

            var tokenTypeName = jobject.Value<string>($"{nameof(Token.Type)}Name");
            var tokenTypeValue = jobject.Value<string>($"{nameof(Token.Type)}Value");

            var rangeLowerBound = jobject.Value<int>($"{nameof(Token.Range)}{nameof(Range.LowerBound)}");
            var rangeUpperBound = jobject.Value<int>($"{nameof(Token.Range)}{nameof(Range.UpperBound)}");

            var providerType = typeof(LanguageProvider).Assembly.GetType($"{typeof(LanguageProvider).Namespace}.{languageProviderName}", throwOnError: true);
            var instance = typeof(Single<>).MakeGenericType(providerType).GetProperty("Instance");
            var languageProvider = (LanguageProvider)instance.GetValue(null);


            var type = (Enum)Enum.Parse(typeof(Token).Assembly.GetType($"{typeof(LanguageProvider).Namespace}.{tokenTypeName}", throwOnError: true), tokenTypeValue);

            return new Token(languageProvider, type, rangeLowerBound, rangeUpperBound, text, isTrivial);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var token = (Token)value;

            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Token.IsTrivial));
            writer.WriteValue(token.IsTrivial);

            writer.WritePropertyName(nameof(Token.Text));
            writer.WriteValue(token.Text);

            writer.WritePropertyName(nameof(Token.LanguageProvider));
            writer.WriteValue(token.LanguageProvider.GetType().Name);

            writer.WritePropertyName($"{nameof(Token.Type)}Name");
            writer.WriteValue(token.Type.GetType().Name);
            writer.WritePropertyName($"{nameof(Token.Type)}Value");
            writer.WriteValue(token.Type.ToString());

            writer.WritePropertyName($"{nameof(Token.Range)}{nameof(Range.LowerBound)}");
            writer.WriteValue(token.Range.LowerBound);

            writer.WritePropertyName($"{nameof(Token.Range)}{nameof(Range.UpperBound)}");
            writer.WriteValue(token.Range.UpperBound);

            writer.WriteEndObject();
        }
    }
}
