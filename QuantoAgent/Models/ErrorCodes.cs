using System;
namespace QuantoAgent.Models {
    public static class ErrorCodes {
        public static readonly string InternalServerError = "INTERNAL_SERVER_ERROR";
        public static readonly string NotFound = "NOT_FOUND";
        public static readonly string EmailAlreadyInUse = "EMAIL_ALREADY_IN_USE";
        public static readonly string NoDataAvailable = "NO_DATA_AVAILABLE";
        public static readonly string InvalidLoginInformation = "INVALID_LOGIN_INFORMATION";
        public static readonly string NotLogged = "NOT_LOGGED";
        public static readonly string AlreadyExists = "ALREADY_EXISTS";
        public static readonly string PermissionDenied = "PERMISSION_DENIED";
        public static readonly string InvalidTokenType = "INVALID_TOKEN_TYPE";
        public static readonly string InvalidFieldData = "INVALID_FIELD_DATA";
        public static readonly string AlreadyClient = "ALREADY_CLIENT";
        public static readonly string AlreadyPaid = "ALREADY_PAID";
        public static readonly string PaymentError = "PAYMENT_ERROR";
        public static readonly string InsufficientFunds = "INSUFFICIENT_FUNDS";
        public static readonly string BankingSystemOffline = "BANKING_SYSTEM_OFFLINE";
        public static readonly string OutdatedAPI = "OUTDATED_API";
        public static readonly string BankNotSupported = "BANK_NOT_SUPPORTED";
        public static readonly string VaultSystemOffline = "VAULT_SYSTEM_OFFLINE";
        public static readonly string ServerIsBusy = "SERVER_IS_BUSY";
        public static readonly string Revoked = "REVOKED";
        public static readonly string AlreadySigned = "ALREADY_SIGNED";
        public static readonly string Rejected = "REJECTED";
        public static readonly string OperationNotSupported = "OPERATION_NOT_SUPPORTED";
        public static readonly string GraphQLError = "GRAPHQL_ERROR";
        public static readonly string OperationLimitExceeded = "OPERATION_LIMIT_EXCEEDED";
        public static readonly string InvalidTransactionDate = "INVALID_TRANSACTION_DATE";
        public static readonly string InvalidSignature = "INVALID_SIGNATURE";
        public static readonly string NotImplemented = "NOT_IMPLEMENTED";
        public static readonly string SealedStatus = "SEALED";

    }
}
