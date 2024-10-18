using Domain.Shared;

namespace Domain.Errors;

public static class ApplicationErrors
{

    public static class BusinessCards
    {
        public static class Email
        {
            public static readonly Error InvalidFormat = new("email InvalidFormat", "The email format is invalid.");
        }

        public static class Queries
        {
            public static readonly Error BusinessCardNotFound = new("BusinessCard Not Found", "No such BusinessCard in the cache or database");
            public static readonly Error NoBusinessCardsFound = new("No Business Cards Found", "No business cards found for the given IDs.");
        }

        public static class Commands
        {

            public static readonly Error BusinessCardCreationFailed = new("BusinessCard Creation Failed","Failed to create the BusinessCard." ); 
            public static readonly Error BusinessCardNotFound = new("BusinessCard Not Found", "BusinessCard Not Found");
            public static readonly Error AtLeastOneIdRequired = new("AtLeastOneIdRequired", "At least one ID is required in the list.");
            public static readonly Error MissingRequiredFields = new( "MissingRequiredFields", "One or more required fields are missing or invalid."
            );

        }


    }

    public static class File
    {
        public static readonly Error EmptyFile = new("Empty File", "File is required and cannot be empty.");
        public static readonly Error FileSizeExceeds_1MB = new("File Size Exceeds", "File size exceeds the maximum allowed size of 1 MB.");
        public static readonly Error ParsingXmlError = new("XML Parsing Error", "Error while parsing XML file.");
        public static readonly Error UnsupportedFileFormat = new("Unsupported File Format", "Unsupported file format.");

        //For CSV File Format
        public static readonly Error UnsupportedCsvFormat = new("Unsupported CSV Format", "Unsupported CSV format. No semicolon or comma delimiter found.");
        public static readonly Error InvalidCsvFormat = new("Invalid CSV Format", "Invalid CSV format. Not enough fields.");
        public static readonly Error InvalidFormat = new("Invalid Format", "Invalid format. Not enough fields.");

        public static readonly Error UnsupportedExportFormat = new("Unsupported Export Format", "The export format is unsupported.");
        public static readonly Error NoFileUploaded = new("NoFileUploaded", "No file was uploaded.");
        public static readonly Error InvalidFileUploaded = new("InvalidFileUploaded", "Invalid file was uploaded.");

        public static readonly string FileExtensionError = "File extension is not allowed. Allowed extensions are: {0}";
        public static readonly Error FileSizeExceeded = new("FileSizeExceeded", "File size cannot exceed {0} MB.");


    }


    public static class Image
    {
        public static readonly Error UnsupportedImageFormat = new("UnsupportedImageFormat", "Unsupported image format. Only PNG, JPG, and GIF are allowed.");
        public static readonly Error ImageConversionError = new("ImageConversionError", "Error while converting image to Base64.");

    }

    public static class QrCode
    {
        public static readonly Error InvalidQrCodeImage = new("InvalidQrCodeImage", "Invalid or empty QR code image.");
        public static readonly Error QrCodeDecodingFailed = new("QrCodeDecodingFailed", "Failed to decode the QR code.");
        public static readonly Error QrCodeGenerationFailed = new("QrCodeGenerationFailed", "Failed to generate the QR code.");
        public static readonly Error InvalidBusinessCardData = new("InvalidBusinessCardData", "Invalid business card data extracted from the QR code.");
        public static readonly Error NoQRCodeDetected = new("NoQRCodeDetected", "No QR code detected.");

    }

   

}