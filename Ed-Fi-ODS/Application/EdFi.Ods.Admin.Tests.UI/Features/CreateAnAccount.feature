Feature: Create an account
	In order to manage my API keys
	As a software vendor wanting to integrate with the Ed-Fi REST API
	I want to create an account

Scenario: Create an account without logging in
	Given No users are logged in
	When I visit the "Create Account" page
	Then I should see text that says "Please Login"

Scenario: Login
	Given No users are logged in
	When I log in with email "sarauserdlptest@gmail.com" and password "SaraUser"
	Then I should see the text "Account (SaraUser)" in the masthead
	And I should not see the text "Manage Accounts" in the masthead

Scenario: Login as Administrator
	Given No users are logged in
	When I log in with email "joeismad2093@gmail.com" and password "JoeAdmin"
	Then I should see the text "Account (JoeAdmin)" in the masthead
	And I should see the text "Manage Accounts" in the masthead

Scenario: Create an account twice without activation
	Given I am logged in successfully with email "joeismad2093@gmail.com" and password "JoeAdmin"
	And The account "edfitestuser@gmail.com" does not exist
	When I create an account with name "John Doe" and email "edfitestuser@gmail.com"
	And I create an account with name "John Doe" and email "edfitestuser@gmail.com"
	Then I should see an error that says "Use the link below to send the confirmation link again."
	
Scenario: Attempt to recreate an account that has already been activated
	Given I am logged in successfully with email "joeismad2093@gmail.com" and password "JoeAdmin"
	And The account "edfitestuser@gmail.com" does not exist
	When I create an account with name "John Doe" and email "edfitestuser@gmail.com"
	And I log out
	And I follow the activation link sent to "edfitestuser@gmail.com"
	And I log in successfully with email "joeismad2093@gmail.com" and password "JoeAdmin"
	And I create an account with name "John Doe" and email "edfitestuser@gmail.com"
	Then I should see an error that says "User already exists with this email address."

Scenario: Resending account activation for an unactivated account
	Given I am logged in successfully with email "joeismad2093@gmail.com" and password "JoeAdmin"
	And The account "edfitestuser@gmail.com" does not exist
	When I create an account with name "John Doe" and email "edfitestuser@gmail.com"
	And I resend account activation to "edfitestuser@gmail.com"
	Then I should see text that says "Email Sent"

Scenario: Resending account activation for an already activated account
	Given I am logged in successfully with email "joeismad2093@gmail.com" and password "JoeAdmin"
	And The account "edfitestuser@gmail.com" does not exist
	When I create an account with name "John Doe" and email "edfitestuser@gmail.com"
	And I log out
	And I follow the activation link sent to "edfitestuser@gmail.com"
	And I activate the account with password "3#SomeString"
	And I log in successfully with email "joeismad2093@gmail.com" and password "JoeAdmin"
	And I resend account activation to "edfitestuser@gmail.com"
	Then I should see a warning that says "The account with email address 'edfitestuser@gmail.com' has already been confirmed. Use the password reset if the password has been lost."

Scenario: Resending account activation for nonexistent account
	Given I am logged in successfully with email "joeismad2093@gmail.com" and password "JoeAdmin"
	And The account "edfitestuser@gmail.com" does not exist
	When I resend account activation to "edfitestuser@gmail.com"
	Then I should see a warning that says "Could not locate an account with email address 'edfitestuser@gmail.com'."

Scenario: Full Account Creation Cycle
	Given The account "billdlptestuser@gmail.com" does not exist
	And I am logged in successfully with email "joeismad2093@gmail.com" and password "JoeAdmin"
	When I create an account with name "Bill" and email "billdlptestuser@gmail.com"
	Then I should see text that says "Email Sent"
	When I log out
	And I follow the activation link sent to "billdlptestuser@gmail.com"
	Then I should see text that says "Activate Account"
	When I activate the account with password "3#SomeString"
	Then I should see text that says "Account Activated"
	When I log in successfully with email "billdlptestuser@gmail.com" and password "3#SomeString"
	Then I should see the text "Account (Bill)" in the masthead

Scenario: Use Account Activation Twice
	Given The account "billdlptestuser@gmail.com" does not exist
	And I am logged in successfully with email "joeismad2093@gmail.com" and password "JoeAdmin"
	When I create an account with name "Bill" and email "billdlptestuser@gmail.com"
	Then I should see text that says "Email Sent"
	When I log out
	And I follow the activation link sent to "billdlptestuser@gmail.com"
	Then I should see text that says "Activate Account"
	When I activate the account with password "3#SomeString"
	And I reuse the activation link sent to "billdlptestuser@gmail.com"
	Then I should see a warning that says "This account has already been confirmed. Would you like to reset your password?"