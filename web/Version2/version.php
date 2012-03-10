<meta http-equiv="expires" content="0">
<meta http-equiv="Pragma" content="no-cache">
<title>TestRecorder Version</title>
<H2>WatiN Test Recorder</H2>
<?php
$currentversion = array(2,0,9,603);
$versionstring = $_GET["version"];
//$versionstring = "1.2.3.4";
print "Your version is $versionstring.<P>\n";

$userversion = split("\.",$versionstring);

print "\n\n";

if ($currentversion[0]>$userversion[0]
	|| $currentversion[1]>$userversion[1]
	|| $currentversion[2]>$userversion[2]
	|| $currentversion[3]>$userversion[3])
	{
		print "A newer version ($currentversion[0].$currentversion[1].$currentversion[2].$currentversion[3]) is available from ";
		print "<a href=https://sourceforge.net/project/showfiles.php?group_id=193118>the SourceForge website</a>.";
	}
	else
	{
		print "Your version is up to date.";
	}
?>