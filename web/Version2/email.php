<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" "http://www.w3.org/TR/REC-html40/loose.dtd">
<?php

/*
Name:                     custommail.php
Revision:                 1.0
Author:                   R.J. Vrijhof
Date:                     2000-Aug-26
Uses:                     HTML 4 Transitional, JavaScript 1.1 (/1.2), PHP (should work in all
                          versions, on all platforms)
PHP Include files:        class.Validator.php3
JavaScript Include files: validate.js
Email address:            R.J.Vrijhof@bigfoot.com
Web site:                 http://www.rjvrijhof.com/ or http://richy.webhop.net/
Download from:            http://richy.dyndns.org/download/


This PHP script enables your web site visitors to send an email to the webmaster ($SERVER_ADMIN) by
means of a browser, instead of the dreaded mailto: link you normally see in web sites. Couldn't find
any script that lived up to my expectations, so I ended up doing it myself.
It tries to check the form fields as thoroughly as possible, so you have a better chance of not
receiving any nonsense, "test" mails, because people have to think a little before typing (just a
very little, but I will come to the validation restrictions later in this file).
It uses an include file from CDI, named Validator 1.2 (filename: class.Validator.php3), bit tweaked
to get it working on my web site and removed the name of the function out of the error messages to
have them better incorporated in my own error messages, but you could better take the original one
because I have Apache/PHP running on a NT server, not on a *NIX machine, so a bit out of the
ordinary, compared to all those *NIX/Linux Apaches out there. But it could also be that my tweaked
version works better for you, as it does for me.
It also uses a JavaScript include file, which I pirated from some web page somewhere on the
internet. It is called validate.js and you guessed it, it also contains functions for form field
validation.

These are the restrictions the form fields are submitted to:
-All fields are required (but the subject and body fields already have content originally)
-"Name" must be at least 3 characters, and may only contain alphanumeric characters and spaces
-"Email address" must be at least 5 characters, and it is checked reasonably thoroughly whether it
 is not a fake address
-"Subject" must be at least 2 characters
-"Body" must be at least 2 characters also

As you've guessed it, the script uses a combination of JavaScript/PHP techniques to validate the
form. As much as possible is done by the browser through JavaScript, and it only uses PHP to
check/validate the email address even more.

****************************************************************************************************
Revision history
****************************************************************************************************
1.0     2000-08-26  First version.
*/

?>
<html>
<!--
Name:                     custommail.php
Revision:                 1.0
Author:                   R.J. Vrijhof
Date:                     2000-Aug-26
-->

<head>

<title>Send email to <?php echo $SERVER_ADMIN;?></title>

<style type="text/css">
<!--
body {
  background-color: #FFFFFF;
  font-family: Times New Roman, Courier, serif;
  font-size: 12pt;
  font-style: normal;
  font-variant: normal;
  font-weight: normal
}
h1 {
  font-family: Verdana, Arial, sans-serif;
  font-size: 16pt;
  font-style: normal;
  font-variant: normal;
  font-weight: bold
}
td {
  font-family: Times New Roman, Courier, sans-serif;
  font-size: 12 pt;
  font-style: normal;
  font-variant: normal;
  font-weight: normal
}
input {
  font-family: Verdana, Arial, sans-serif;
  font-size: 10pt;
  font-style: normal;
  font-variant: normal;
  font-weight: normal
}
textarea {
  font-family: Verdana, Arial, sans-serif;
  font-size: 10pt;
  font-style: normal;
  font-variant: normal;
  font-weight: normal
}
.bold {
  font-weight: bold
}
.italic {
  font-style: italic
}
-->
</style>

<!-- Include validate.js for validation of the form fields -->
<script type="text/javascript" language="javascript" src="/scripts/validate.js"></script>

</head>

<body bgcolor="#FFFFFF">

<div align="center">
  <h1>Send email to <?php echo $SERVER_ADMIN;?></h1>
</div>
<?php

// Include class.Validator.php3 for extra, server-sided validation/checking of email address
include("class.Validator.php3");
$validator = new Validator;

// Check/validate email address with PHP (only when filled in, so the form is already submitted)
$faOK = false;
if ($from_addr && $validator->is_email($from_addr))
  $faOK = true;

/***************************************************************************************************
Executive part of the script (the form is already submitted and the email address is already
checked, so the email is sent and a confirmation is shown). If the email address is not alright, an
error message is shown, otherwise the confirmation is shown.
***************************************************************************************************/
if ($action == "send" && $from_name && $from_addr && $subject && $body && $faOK) {
// Strip the form fields from any HTML tags (just in case...)
  $from_name = $validator->strip_html($from_name);
  $from_addr = $validator->strip_html($from_addr);
  $subject   = $validator->strip_html($subject);
  $body      = $validator->strip_html($body);

// Merge the name and email address fields as follows: "John Smith" <j.smith@ema.com>
  if($from_name != "")
    $from = "\"$from_name\" <$from_addr>";
  else
    $from = $from_addr;

// Send the mail
$sendto = "watinrecorder@gmail.com";
  mail($sendto, $subject, $body, "From: $from\nX-Mailer: PHP/" . phpversion() . "\n");

?>
<!-- Display confirmation text -->
<div align="center">
<h2>Message sent:</h2>
<span class="italic">From:</span> <span class="bold"><strong><?php echo htmlspecialchars($from);?></strong></span><br>
<span class="italic">Subject:</span> <span class="bold"><strong><?php echo htmlspecialchars($subject);?></strong></span><br>
<hr width="40%">
<span class="italic">Body:</span><br>
<span class="bold"><strong><pre><?php echo htmlspecialchars($body);?></pre></strong></span><br>
</div>
<br>
<a href="/sitemap/" onmouseover="self.status='Go to site map!';return true;" onmouseout="self.status='';return true;" target="_top">Sitemap</a>
<?php

/***************************************************************************************************
Submit part of the script, where the form is shown to fill in and send the email. Contains also some
JavaScript to validate the fields before submitting, which will alert the user in case any failures
to satisfy the restrictions set on the fields, are detected.
***************************************************************************************************/
} else {

// Give the user the choice to send it with his/her email client (the old fashioned way, with a "mailto:" link)
echo "<div align=\"center\">Send this email with your <a href=\"mailto:$SERVER_ADMIN\">email client</a>. Or send it with this form:</div><br>";

// Display the error message concerning an invalid email address (of course only IF it is invalid)
if ($from_addr && !$faOK)
  echo "<div align=\"center\">Error in email address: $validator->ERROR</div><br>";

?>
<script type="text/javascript" language="javascript">
<!--
/* Simple onsubmit() function to set the restrictions and check/validate the form fields before
submitting
*/
function validate() {
// Create booleans that will help set up the pop-up alert window with the error message(s)
  var fnOK = false;
  var faOK = false;
  var suOK = false;
  var boOK = false;
// The name field must be at least 3 characters and contain only alphanumeric characters and spaces
  if (isMinLen(document.mail.from_name, 3) && isAlphaNumeric(document.mail.from_name.value))
    fnOK = true;
// The email address must be at least 5 characters and validate the email address as much as
// possible
  if (isMinLen(document.mail.from_addr, 5) && isValidEmail(document.mail.from_addr.value))
    faOK = true;
// Subject may not be empty, so I check for 2 characters
  if (isMinLen(document.mail.subject, 2))
    suOK = true;
// Same for the email body
  if (isMinLen(document.mail.body, 2))
    boOK = true;
// All fields checked and OK
  if (fnOK && faOK && suOK && boOK) {
    return true;
  } else {
// Build up the alert error message text
    var msgStr = "Error(s) in form:\n";
// Error in name field
    if (!fnOK) {
      document.mail.from_name.focus();
      msgStr += "-Name is required and may only contain spaces & alphanumeric characters\n";
    }
// Error in email address field
    if (!faOK) {
      document.mail.from_addr.focus();
      msgStr += "-Email address is required and must be valid\n";
    }
// Error in subject field
    if (!suOK) {
      document.mail.subject.focus();
      msgStr += "-Subject is required\n";
    }
// Error in mail body field
    if (!boOK) {
      document.mail.body.focus();
      msgStr += "-Email body is required\n";
    }
// Pop up an alert window
    alert(msgStr);
    return false;
  }
}
// -->
</script>

<!-- This is the fill-in form -->
<form name="mail" id="mail" action="custommail.php" method="post" onsubmit="return validate()">
<div align="center">
  <table border="0">
    <tr>
      <td><font face="Times New Roman, Courier New, serif">Name</font></td>
      <td>
        <input type="hidden" name="action" id="action" value="send">
        <input type="text" name="from_name" id="from_name" size="40" maxlength="40" value="<?php echo $from_name;?>">
      </td>
    </tr>
    <tr>
      <td><font face="Times New Roman, Courier New, serif">Email address</font></td>
      <td>
        <input type="text" name="from_addr" id="from_addr" size="40" maxlength="40" value="<?php if ($faOK) echo $from_addr;?>">
      </td>
    </tr>
    <tr>
      <td><font face="Times New Roman, Courier New, serif">Subject</font></td>
      <td>
        <input type="text" name="subject" id="subject" size="40" maxlength="40" value="<?php if ($subject) echo $subject; else echo "Mail sent from Richy's web site";?>">
      </td>
    </tr>
    <tr>
      <td colspan="2">
        <font face="Verdana, Arial, sans-serif"><textarea name="body" id="body" rows="10" cols="55" wrap="virtual"><?php if ($body) echo $body; else echo "Type your text here.\nOnly plain text, no HTML!";?></textarea></font>
      </td>
    </tr>
    <tr>
      <td></td>
      <td>
        <input type="submit" value="Send!">
      </td>
    </tr>
  </table>
</div>
</form>

<script type="text/javascript" language="javascript">
<!--
document.mail.from_name.focus();
// -->
</script>
<?php

}

?>

</body>

</html>
