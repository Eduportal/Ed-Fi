Feature: ForgotPassword
	

Scenario: Forgot Password so Reset Password
	Given There exists a regular user with name "BillTheTitan" password "SmallB0b!" email "billdlptestuser@gmail.com"
	And No users are logged in
	And I am on the forgot password page
	When I send the reset email to "billdlptestuser@gmail.com"
	And I follow the forgot password link sent to "billdlptestuser@gmail.com"
	And I reset the password to "1!Aaaaaa"
	And I log in with email "billdlptestuser@gmail.com" and password "1!Aaaaaa"

