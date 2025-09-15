using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace SimpleIdentity.Encryption;

public class TokenHasher
{
    //must be 32 chars at least and kept safer than this
    private const string secret = "devsecretkey1.2.3.4.5.6.7.8.9.10";
    private const char segmentDelimiter = '.';
    private const int tokenValidity = 120;

    private const string _typ = "JWT";
    private const string _alg = "HS256";


    public class Header
    {
        public string? typ { get; set; }
        public string? alg { get; set; }

    }

    public class Payload
    {
        public int? uid { get; set; }
        public long? iat { get; set; }
        public long? exp { get; set; }

    }



    public static string CreateMessage(int UserId)
    {

        var header = new { typ = "JWT", alg = "HS256" };

        string jsonHeader = JsonSerializer.Serialize(header);

        byte[] headerBytes = Encoding.UTF8.GetBytes(jsonHeader);
        string base64Header = WebEncoders.Base64UrlEncode(headerBytes);

        var payload = new Payload
        {
            uid = UserId,
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddMinutes(tokenValidity).ToUnixTimeSeconds()

        };

        string jsonPayload = JsonSerializer.Serialize(payload);

        byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);
        string base64Payload = WebEncoders.Base64UrlEncode(payloadBytes);


        return string.Join(segmentDelimiter, base64Header, base64Payload);

    }


    private static byte[] CreateSignature(string message)
    {
        var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        byte[] hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(message));
        return hash;

    }

    public static string CreateJWT(int UserId)
    {
        string message = CreateMessage(UserId);

        string signature = WebEncoders.Base64UrlEncode(CreateSignature(message));

        string JWT = string.Join(segmentDelimiter, message, signature);

        return JWT;

    }
    public static Header? GetHeaderFromBase64String(string base64String)
    {
        try
        {
            byte[] headerBytes = WebEncoders.Base64UrlDecode(base64String);
            string headerString = Encoding.UTF8.GetString(headerBytes);
            var headerObject = JsonSerializer.Deserialize<Header>(headerString);

            return headerObject;
        }
        catch
        {
            return null;
        }

    }
    public static Payload? GetPayloadFromBase64String(string base64String)
    {
        try
        {
            byte[] payloadBytes = WebEncoders.Base64UrlDecode(base64String);
            string payloadString = Encoding.UTF8.GetString(payloadBytes);
            var payload = JsonSerializer.Deserialize<Payload>(payloadString);

            return payload;
        }
        catch
        {
            return null;
        }

    }


    public static byte[] GetBytesFromBase64String(string base64String)
    {
        try
        {
            byte[] signatureBytes = WebEncoders.Base64UrlDecode(base64String);

            return signatureBytes;
        }
        catch
        {
            byte[] signatureBytes = [];
            return signatureBytes;
        }

    }


    public static JWTValidationResult ValidateJWT(string JWT)
    {
        JWTValidationResult JWTresult = new();


        string[] segments = JWT.Split(segmentDelimiter);

        if (segments.Length != 3)
        {
            JWTresult.ErrorMessage = "JWT segments count failure. It should contain three base64 encoded segments separated by a dot: header.payload.signature";

            return JWTresult;
        }

        if (segments[0].Length == 0 || segments[1].Length == 0 || segments[2].Length == 0)
        {
            JWTresult.ErrorMessage = "Missing some of: header, payload or signature";
            
            return JWTresult;
        }

      
        var header = GetHeaderFromBase64String(segments[0]);
        var payload = GetPayloadFromBase64String(segments[1]);

        if ((header == null) || (payload == null))
        {
            JWTresult.ErrorMessage = "Message decode error. Malformed JSON or not valid base64 urlsafe encoding.";

            return JWTresult;
        }

        if (header.typ != _typ | header.alg != _alg)
        {
            JWTresult.ErrorMessage = "Cannot validate header info.";

            return JWTresult;
        }

        if ((payload.iat == null) ||
        (payload?.iat) < DateTimeOffset.UtcNow.AddMinutes(-tokenValidity).ToUnixTimeSeconds() ||
        (payload?.iat) > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            JWTresult.ErrorMessage = "Payload expiry or issued in the future or missing.";

            return JWTresult;
        }

        if (payload?.uid == null)
        {
            JWTresult.ErrorMessage = "Payload uid must have a value.";

            return JWTresult;
        }

        string message = string.Join(segmentDelimiter, segments[0], segments[1]);

        byte[] hashToValidate = CreateSignature(message);

        byte[] signatureBytes = GetBytesFromBase64String(segments[2]);

        if (signatureBytes.Length == 0)
        {
            JWTresult.ErrorMessage = "Signature is not in base64 urlsafe encoded format.";

            return JWTresult;
        }

        //        Console.WriteLine("hash to validate: " + WebEncoders.Base64UrlEncode(hashToValidate));
        //        Console.WriteLine("signature from token: " + segments[2]);

        if (CryptographicOperations.FixedTimeEquals(hashToValidate, signatureBytes) == true)
        {

            JWTresult.IsValid = true;
            JWTresult.UserID = (int)payload.uid;

            return JWTresult;
        }
        else
        {

            JWTresult.ErrorMessage = "Signature verification failed.";

            return JWTresult;
        }

    }
}
