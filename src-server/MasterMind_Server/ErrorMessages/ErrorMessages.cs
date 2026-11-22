namespace MasterMind_Server.Exceptions
{
    internal enum ErrorMessages
    {
        DataBaseError,
        HashError,
        InvalidImage,
        ImageTooLarge,
        ImageProcessingError,
        PlayerNotFound,
        RoomNotFound,
        NotPlayerOne,
        PlayersNotReady,
        MatchNotStarted,
        InvalidCode,
        TriesExhausted,
        PlayerNotInRoom,
        SecretCodeNotFound,
        SelfReport,
        InvalidReportReason,
        AlreadyReported
    }
}
