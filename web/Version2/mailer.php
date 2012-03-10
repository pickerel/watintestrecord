<?php

print "<BR>Subject: ".$subject;
print "<BR>Body: ".$body;
print "<BR>From: ".$from;

$headers = "From: info@example.com \r\n"; 
$headers.= "Content-Type: text/html; charset=ISO-8859-1 "; 
$headers .= "MIME-Version: 1.0 "; 

$sendto = "watinrecorder@gmail.com";
imap_mail($sendto, $subject, $body, $headers);

?>