BEGIN
	EXEC('CREATE PROCEDURE dbo.AccessTokenIsValid 
			@AccessToken uniqueidentifier,
			@TimeoutMinutes int = 30
	AS
		BEGIN
			DELETE FROM dbo.ClientAuthorizationCodes WHERE DATEDIFF(MINUTE, Expiration, GETDATE()) > @TimeoutMinutes;
			
			DELETE FROM dbo.ClientAccessTokens WHERE DATEDIFF(MINUTE, Expiration, GETDATE()) > @TimeoutMinutes;
			
			UPDATE ClientAccessTokens SET Expiration = DATEADD(MINUTE, @TimeoutMinutes, GETDATE()) WHERE ID = @AccessToken
			
			SELECT ac.[Name], ac.[Key], alea.[LocalEducationAgencyId], app.ClaimSetName, v.NamespacePrefix
			FROM ClientAccessTokens cat 
				INNER JOIN ApiClients ac ON cat.ApiClient_ApiClientId = ac.ApiClientId AND cat.ID = @AccessToken
				INNER JOIN Applications app	ON app.ApplicationID = ac.Application_ApplicationID
				LEFT OUTER JOIN Vendors v ON v.VendorId = app.Vendor_VendorId
				--Outer join so client key is always returned even if no LEAs have been enabled
				LEFT OUTER JOIN [dbo].[ApiClientApplicationLocalEducationAgencies] aleaac ON aleaac.ApiClient_ApiClientId = cat.ApiClient_ApiClientId
				LEFT OUTER JOIN [dbo].[ApplicationLocalEducationAgencies] alea ON alea.ApplicationLocalEducationAgencyId = aleaac.ApplicationLocalEducationAgency_ApplicationLocalEducationAgencyId
		END; ')
END