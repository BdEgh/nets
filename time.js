let fs = require("fs");
let shift = fs.readFileSync("conf", "utf8");

const http = require('http');
const port = 3000;

const requestHandler = (request, response) => {
	let time = new Date(Date.now() + shift * 1000);
	response.write("<html>");
    response.write('<meta http-equiv="refresh" content="1" >');
	response.write("<meta charset=\"utf-8\" />");
	response.write("<body><h2>" + time.getDate() + ' ' + (time.getMonth() + 1) + ' ' + time.getFullYear() + "<br>" + time.getHours() + ' ' + time.getMinutes() + ' ' + time.getSeconds() + "</h2></body>");
	response.write("</html>");
	response.end();
}

const server = http.createServer(requestHandler);
server.listen(port, (err) => {
	if (err)
		return console.log('something bad happened', err);
	console.log(`server is listening on ${port}`);
});
