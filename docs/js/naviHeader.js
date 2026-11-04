function writeNaviHeader( ){
	var html = "";
	html += '<nav ID="headerNav">';
		html += '<ul>';
			html += '<li><a href="index.html">Home</a></li>';
			html += '<li><a href="page2.html">Sudoku Algorithm</a></li>';
			html += '<li><a href="page17.html">Download</a></li>';
			html += '<li><a href="page19.html">about this</a></li>';
		html += '</ul>';
	html += '</nav>';
	document.write(html);
}