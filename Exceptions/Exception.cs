using System;

namespace Hospital_Project.Exceptions
{
    internal class Exception_Class(string message) : Exception(message)
    {
        public static Exception_Class Invalid_Quantity_Of_Characters()
        {
            return new Exception_Class("The quantity of characters used in password is invalid.");
        }
    }
}