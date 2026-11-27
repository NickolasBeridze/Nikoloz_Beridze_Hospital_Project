using System;
using Hospital_Project.Enums;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Exceptions
{
    internal class Exception_Class(string message) : Exception(message)
    {
        public static Exception_Class Invalid_Quantity_Of_Characters()
        {
            return new Exception_Class(T("Password must be at least 6 characters long."));
        }

        public static Exception_Class Password_Empty()
            => new(T("Password cannot be empty."));

        public static Exception_Class Password_Same_As_Current()
            => new(T("The new password must be different from the current password."));

        public static Exception_Class Passwords_Do_Not_Match()
            => new(T("The new password and confirmation do not match."));

        public static Exception_Class Invalid_Credentials(int attemptsLeft)
            => new($"{T("Invalid email or password.")} {T("Attempts left:")} {attemptsLeft}");

        public static Exception_Class Account_Locked(int seconds)
            => new($"{T("Too many failed attempts. Try again in")} {seconds} {T("seconds.")}");

        public static Exception_Class Duplicate_Id(int id)
            => new($"{T("A record with this ID already exists:")} {id}");

        public static Exception_Class Record_Not_Found(string what)
            => new($"{T("Record not found:")} {what}");

        public static Exception_Class Invalid_Status_Transition(AppointmentStatus from, AppointmentStatus to)
            => new($"{T("Invalid status change:")} {from} -> {to}");

        public static Exception_Class Scheduling_Conflict(string reason)
            => new(reason);

        public static Exception_Class Invalid_Date(string reason)
            => new(reason);

        public static Exception_Class Invalid_Amount(string reason)
            => new(reason);

        public static Exception_Class Invalid_Input(string reason)
            => new(reason);
    }
}
