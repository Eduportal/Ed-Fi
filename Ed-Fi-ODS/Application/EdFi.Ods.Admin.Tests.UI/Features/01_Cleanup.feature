Feature: 01_Cleanup
	Things that need to be cleaned up

Scenario: Remove Zombie Sandboxes
	Given No users are logged in
	When I log in with email "Joeismad2093@gmail.com" and password "JoeAdmin"
	And I visit the "Orphan Report" page
	And I clear all orphan databases
	Then I should have "0" orphan databases

Scenario: Cleanup Email
	Given Test User Bill has no email in "billdlptestuser@gmail.com"
	And Test User Joe has no email in "joeismad2093@gmail.com"
	And Test User EdFi has no email in "edfitestuser@gmail.com"
	And Test User Sara has no email in "sarauserdlptest@gmail.com"
	When I begin testing
	Then They should not have any emails from DLP

