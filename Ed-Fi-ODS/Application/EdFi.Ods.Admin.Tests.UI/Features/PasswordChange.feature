Feature: Password Change

Background:
	Given I am on the Admin console home page
	And There exists a regular user with name "Bill" password "2@aAaaaa" email "billdlptestuser@gmail.com"
	And I am logged in with email "billdlptestuser@gmail.com" and password "2@aAaaaa"
	And I am on the password change page

Scenario: Change Password
	When I change my password to "1!aAaaaa"
	And I log out
	And I log in with email "billdlptestuser@gmail.com" and password "1!aAaaaa"
	Then "Bill" should be logged in

Scenario: Password Changing Needs Current Password
	When I click OK
	Then I should see a warning that says "CurrentPassword is required."

Scenario: Password Changing Needs New Password
	When I type in the invalid old password
	And I click OK
	Then I should see a warning that says "NewPassword is required."

Scenario: Password Changing Needs Password Confirmation
	When I type in the invalid old password
	And I type in the Invalid new password
	And I click OK
	Then I should see a warning that says "ConfirmPassword is required."

Scenario: Password Changing Needs New passwords to match
	When I type in the invalid old password
	And I type in the Invalid new password
	And I type in the Correct confirm password
	And I click OK
	Then I should see a warning that says "New Password and Confirm Password must match."

Scenario: Password Changing Needs New Password to be valid
	When I type in the invalid old password
	And I type in the Invalid new password
	And I type in the Invalid confirm password
	And I click OK
	Then I should see a warning that says "NewPassword is invalid"

Scenario: Password Changing Needs Correct Current Password
	When I type in the invalid old password
	And I type in the Correct new password
	And I type in the Correct confirm password
	And I click OK
	Then I should see a warning that says "The Current Password supplied is incorrect"