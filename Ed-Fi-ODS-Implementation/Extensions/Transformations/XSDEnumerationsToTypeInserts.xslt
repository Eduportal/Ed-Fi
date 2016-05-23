<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" version="1.0">
  <xsl:output method="text" version="1.0" encoding="UTF-8" indent="yes" />
  <xsl:template match="/">
    <xs:text>
    ----------------------------------------------------------------------------------------------------------------------------
    -- NOTE: This script is automatically generated using the 'XSDEnumerationsToTypeInserts.xslt' transformation.
    -- The Linqpad script 'Execute-XSLT-transforms-against-Ed-Fi-Core-schema.linq' can be used to interactively regenerate the SQL script.
    ----------------------------------------------------------------------------------------------------------------------------
	IF NOT EXISTS(SELECT * FROM edfi.ElectronicMailType)
      BEGIN
    </xs:text>
    <xsl:apply-templates select="//xs:simpleType[@name != 'SchoolYearType' and xs:restriction/xs:enumeration]" />
    <xs:text>
      END
    </xs:text>
  </xsl:template>
  <xsl:template match="xs:simpleType[xs:restriction/xs:enumeration]">
    <xsl:apply-templates select="xs:restriction/xs:enumeration">
      <xsl:with-param name="simpleTypeName" select="@name" />
    </xsl:apply-templates>
  </xsl:template>
  <xsl:template match="xs:enumeration">
    <xsl:param name="simpleTypeName" />
    <xsl:variable name="tableNameWithoutMap">
      <xsl:call-template name="replace-string">
        <xsl:with-param name="text" select="$simpleTypeName" />
        <xsl:with-param name="replace" select="'Map'" />
        <xsl:with-param name="with" select="''" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="tableName">
      <xsl:call-template name="replace-string">
        <xsl:with-param name="text" select="$tableNameWithoutMap" />
        <xsl:with-param name="replace" select="'TypeType'" />
        <xsl:with-param name="with" select="'Type'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="apos">'</xsl:variable>
    <xsl:variable name="apos2">''</xsl:variable>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text>INSERT INTO [edfi].[</xsl:text><xsl:value-of select="$tableName" />] (CodeValue, ShortDescription, [Description]) VALUES ('<xsl:call-template name="replace-string">
      <xsl:with-param name="text" select="substring(@value,1,50)" />
      <xsl:with-param name="replace" select="$apos" />
      <xsl:with-param name="with" select="$apos2" />
    </xsl:call-template>', '<xsl:call-template name="replace-string">
      <xsl:with-param name="text" select="@value" />
      <xsl:with-param name="replace" select="$apos" />
      <xsl:with-param name="with" select="$apos2" />
    </xsl:call-template>', '<xsl:call-template name="replace-string">
      <xsl:with-param name="text" select="@value" />
      <xsl:with-param name="replace" select="$apos" />
      <xsl:with-param name="with" select="$apos2" />
    </xsl:call-template>');
    <xsl:text />
  </xsl:template>
  <xsl:template name="replace-string">
    <xsl:param name="text" />
    <xsl:param name="replace" />
    <xsl:param name="with" />
    <xsl:choose>
      <xsl:when test="contains($text,$replace)">
        <xsl:value-of select="substring-before($text,$replace)" />
        <xsl:value-of select="$with" />
        <xsl:call-template name="replace-string">
          <xsl:with-param name="text" select="substring-after($text,$replace)" />
          <xsl:with-param name="replace" select="$replace" />
          <xsl:with-param name="with" select="$with" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>