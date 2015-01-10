﻿//-----------------------------------------------------------------------
// <copyright file="User.cs" company="Sully">
//     Copyright (c) Johnathon Sullinger. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace SampleUniversalApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Sullinger.ValidatableBase.Models;
    using Sullinger.ValidatableBase.Models.ValidationRules;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The model used to hold a user email and password.
    /// The INotifyPropertyChanged implementation should probably be pulled out in to a Facade at some-point.
    /// </summary>
    public class User : ValidatableBase, INotifyPropertyChanged
    {
        public User()
        {
            this.Account = new Bank();
        }

        /// <summary>
        /// The Email backing field.
        /// </summary>
        private string email = string.Empty;

        /// <summary>
        /// The Password backing field.
        /// </summary>
        private string password = string.Empty;

        public Bank Account {get;set;}

        /// <summary>
        /// Raised when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the users current balance. 
        /// If the balance falls below the bank minimum required, a warning is generated. Only applicable if the user has an open account.
        /// </summary>
        [ValidateNumberIsGreaterThan(
            ComparisonProperty = "Account.MinimumBalance",
            FailureMessage = "You can not withdraw anymore, you must maintain a minimum balance.",
            ValidationMessageType = typeof(ValidationWarningMessage),
            ValidateIfMemberValueIsValid = "Account.IsOpen")]
        public int CurrentBalance { get; set; }

        /// <summary>
        /// Gets or sets the Email.
        /// </summary>
        [ValidateObjectHasValue(
            LocalizationKey = "User-Email-Validation-Failure-Cannot-be-blank",
            ValidationMessageType = typeof(ValidationErrorMessage))]
        [ValidateWithCustomHandler(
            DelegateName = "ValidateEmailFormat",
            ValidationMessageType = typeof(ValidationErrorMessage),
            /* Key does not exist so validation falls back to the specified FailureMessage */
            LocalizationKey = "User-Email-Validation-Failure-Invalid-Format",
            FailureMessage = "Email address is not properly formatted.")]
        public string Email
        {
            get
            {
                return this.email;
            }

            set
            {
                this.email = value;
                this.OnPropertyChanged("Email");
            }
        }

        /// <summary>
        /// Gets or sets the Password.
        /// </summary>      
        [ValidateStringIsGreaterThan(GreaterThanValue = 6,
            ValidateIfMemberValueIsValid = "Email",
            FailureMessage = "Password must be greater than 6 characters.",
            ValidationMessageType = typeof(ValidationErrorMessage))]
        [ValidateStringIsLessThan(LessThanValue = 20,
            ValidateIfMemberValueIsValid = "Email",
            FailureMessage = "Password must be less than 20 characters.",
            ValidationMessageType = typeof(ValidationErrorMessage))]
        [ValidateWithCustomHandler(
            DelegateName = "IsPasswordCorrectlyFormatted",
            FailureMessage = "Passwords can not use special characters",
            ValidationMessageType = typeof(ValidationErrorMessage))]
        public string Password
        {
            get
            {
                return this.password;
            }

            set
            {
                this.password = value;
                this.OnPropertyChanged("Password");
            }
        }

        [ValidationCustomHandlerDelegate(DelegateName = "IsPasswordCorrectlyFormatted")]
        private IValidationMessage ValidatePasswordFormat(IValidationMessage failureMessage, PropertyInfo property)
        {
            string regExp = @"^[a-zA-Z0-9\s]+$";

            return Regex.IsMatch(this.Password, regExp)
                ? null
                : failureMessage;
        }

        /// <summary>
        /// Raised when a property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Validates that the users email address is valid.
        /// </summary>
        /// <param name="failureMessage">The message that will be returned if validation fails.</param>
        /// <param name="property">The property info fetched when the model was registered for validation</param>
        /// <returns>Returns a validation message if validation fails. Otherwise null is returned.</returns>
        [ValidationCustomHandlerDelegate(DelegateName = "ValidateEmailFormat")]
        private IValidationMessage ValidateEmailIsFormatted(IValidationMessage failureMessage, PropertyInfo property)
        {
            string[] addressParts = this.Email.Split('@');

            if (addressParts.Length < 2)
            {
                return failureMessage;
            }

            string[] domainPiece = addressParts.LastOrDefault().Split('.');
            if (domainPiece.Length < 2)
            {
                return failureMessage;
            }

            return null;
        }
    }
}
