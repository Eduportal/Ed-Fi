BEGIN
	EXEC('CREATE PROCEDURE dbo.AccessTokenIsValid 
			@AccessToken uniqueidentifier,
			@TimeoutMinutes int = 30
	AS
		BEGIN
			DELETE FROM dbo.ClientAuthorizationCodes WHERE DATEDIFF(MINUTE, Expiration, GETDATE()) > @TimeoutMinutes;
			DELETE FROM dbo.ClientAccessTokens WHERE DATEDIFF(MINUTE, Expiration, GETDATE()) > @TimeoutMinutes;
			UPDATE ClientAccessTokens SET Expiration = DATEADD(MINUTE, @TimeoutMinutes, GETDATE()) WHERE ID = @AccessToken
			SELECT AC.[Name], AC.[Key], ALEA.[LocalEducationAgencyId]
			FROM ClientAccessTokens CAT 
			INNER JOIN ApiClients AC
			ON CAT.ApiClient_ApiClientId = AC.ApiClientId AND CAT.ID = @AccessToken
			--Outer join so client key is always returned even if no LEAs have been enabled
			LEFT OUTER JOIN [dbo].[ApplicationLocalEducationAgencyApiClients] ALEAAC
			ON ALEAAC.ApiClient_ApiClientId = CAT.ApiClient_ApiClientId
			LEFT OUTER JOIN [dbo].[ApplicationLocalEducationAgencies] ALEA
			ON ALEA.ApplicationLocalEducationAgencyId = ALEAAC.ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId
		END; ')
END