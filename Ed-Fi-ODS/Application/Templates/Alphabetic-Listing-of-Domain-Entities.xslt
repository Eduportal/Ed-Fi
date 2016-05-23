<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions">
	<xsl:output method="text" version="1.0" encoding="UTF-8" indent="yes"  />
	<xsl:preserve-space elements="true"  />

	<xsl:template match="/">
		<xsl:apply-templates select="//Entity" >
			<xsl:sort select="@table"/>
		</xsl:apply-templates>
	</xsl:template>	
	
	<xsl:template match="*">
<xsl:value-of select="./@table"/><xsl:text>&#xa;</xsl:text>
	</xsl:template>
</xsl:stylesheet>
