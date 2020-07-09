#!/usr/bin/env python
# encoding: utf-8
"""
makeTOC.py

Created by Georg Seifert on 2010-06-23.
Copyright (c) 2010 schriftgestaltung.de. All rights reserved.
"""

import sys
import os, re
from xml.dom.minidom import parse, parseString, Document


def main():
	folder = os.path.join(os.path.dirname(os.path.abspath(__file__)), "FontValidator.help/Contents/Resources/en.lproj/pgs")
	
	dirlist=os.listdir(folder)
	pages = {}
	for html in dirlist:
		#print html
		if (html[-3:] == "htm" and len(html) == 9):
			#print "__", html
			f = open(os.path.join(folder,html), "r")
			lines = f.readlines()
			f.close()
			Title = lines[13]
			Title = re.sub('<[^<]+?>', '', Title)
			Title = Title.strip()
			Name = os.path.splitext(html)[0]
			
			pages[Name] = Title
	
	f = open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "FontValidator.help/Contents/Resources/en.lproj/toc.html"), "w")
	f.write('''\
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">

<head>
	<title>FontValidator Index </title>
	<meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
	<meta name="template" content="index"/>
	<meta name="pagetype" content="index"/>
	<meta name="robots" content="anchors"/>
	<link rel="stylesheet" href="../shrd/styles.css" type="text/css" media="screen">
</head>

<body>
	<!--top navigation area-->
	<div id="navbox" class="gradient">
		<a name="toc"></a>
		<div id="navleftbox">
			<a class="navlink_left" href="help:anchor='access' bookID=FontValidator Help">Start</a>
		</div>
	</div>
	<!--closes navigation area-->
	
	<!--list area-->
	<div id="indexlist">''')
	for key in sorted(pages.keys()):
		Title = pages[key]
		Text = '''\
		<div class="indexitem">
			<div class="indexentrytext">
				<a href="help:anchor=%s bookID=FontValidator Help"><span class="number">%s:</span> %s</a>
			</div>
		</div>\n''' % (key, key, Title);
		f.write(Text.encode("utf-8"))
	f.write('''\
		<br style="clear:both;" />
	</div>
	<!--closes list area-->
</body>
</html>\n''')


if __name__ == '__main__':
	main()

