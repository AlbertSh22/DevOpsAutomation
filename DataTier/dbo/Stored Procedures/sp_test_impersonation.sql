CREATE PROCEDURE [dbo].[sp_test_impersonation]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT SUSER_NAME() [usr]

	RETURN 0; -- SUCCESS
END