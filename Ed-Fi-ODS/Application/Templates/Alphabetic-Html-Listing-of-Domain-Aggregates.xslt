<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions">
	<xsl:output method="html" version="1.0" encoding="UTF-8" indent="yes"  />

	<xsl:template match="/">
			<html>
				<head>
					<title></title>
				</head>
				<body>
					<table border="1">
						<tbody>
							<xsl:apply-templates select="//Aggregate" >
								<xsl:sort select="@root"/>
							</xsl:apply-templates>
						</tbody>
					</table>
				</body>
			</html>
	</xsl:template>	
	
	<xsl:template match="Aggregate">
		<tr valign="top">
			<th align="left"><xsl:value-of select="@root"/></th>
			<td>
				<xsl:apply-templates select="Entity"/>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="Entity">
		<xsl:value-of select="./@table"/><br/>
	</xsl:template>
</xsl:stylesheet>
