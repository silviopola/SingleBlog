using SingleBlog.Dto;

namespace SingleBlog.Entities
{
    public static class RequestValidator
    {
        private const int ContentMaxLength = 1024;

        public static ValidationResult ValidateFields(RequestPost requestPost)
        {
            if (string.IsNullOrEmpty(requestPost.Title))
                return new ValidationResult(false, "Title is empty");

            if (string.IsNullOrEmpty(requestPost.Author))
                return new ValidationResult(false, "Author is empty");

            if (string.IsNullOrEmpty(requestPost.Content))
                return new ValidationResult(false, "Content is empty");

            if (requestPost.Content.Length > ContentMaxLength)
                return new ValidationResult(false, $"Content exceed the max length of {ContentMaxLength} chars");

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateFieldsIfNotNull(RequestPost requestPost)
        {
            if (requestPost.Title != null && requestPost.Title == string.Empty)
                return new ValidationResult(false, "Title is empty");

            if (requestPost.Author != null && requestPost.Author == string.Empty)
                return new ValidationResult(false, "Author is empty");

            if (requestPost.Content != null && requestPost.Content == string.Empty)
                return new ValidationResult(false, "Content is empty");

            if (requestPost.Content != null && requestPost.Content.Length > ContentMaxLength)
                return new ValidationResult(false, $"Content exceed the max length of {ContentMaxLength} chars");

            return new ValidationResult(true);
        }
    }



    public class ValidationResult
    {
        public ValidationResult(bool isValid)
        {
            IsValid = isValid;        
        }

        public ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }

        public bool IsValid { get; }
        public string Message { get; }
    }
}
