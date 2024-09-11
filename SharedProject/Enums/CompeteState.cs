namespace SharedProject.Enums;

public enum CompeteState : uint
{
    Disabled = 0,
    Normal = 1,             // In Progress
    Waiting = 2,            // Waiting for Answer
    Finished = 3,           // Finished Once Compete
    Rejected = 4            // Rejected Challenge
}
