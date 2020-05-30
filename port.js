var net = require('net');
var Socket = net.Socket;
var host = '127.0.0.1';

async function checkPortStatus (port) {
  let timeout = 400;
  let connectionRefused = false;

  let socket = new Socket();
  let status = null;
  let error = null;

  socket.on("connect", function () {
    status = "open";
    socket.destroy();
  });

  socket.setTimeout(timeout);
  socket.on("timeout", function () {
    status = "closed";
    error = "Timeout (" + timeout + "ms)";
    socket.destroy();
  });

  socket.on("error", function (exception) {
    error = exception.code;
    status = "closed";
  });

  socket.on("close", function (exception) {
    let resp = "Port " + port + ' ' + status + '.';
	if (error !== null)
		resp += " Error occured: " + error;
	console.log(resp);
  });

  socket.connect(port, host);
}

let args = process.argv.slice(2);
let from = 1, to = 65535;
if (Number.isInteger(args[0] * 1) && Number.isInteger(args[1] * 1) && args[0] <= args[1]) {
	from = args[0];
	to = args[1];
}
else
	console.log("Incorrect port input. Checking default range.");

if (args[2] != undefined)
	host = args[2];

console.log("TCP port check on " + host);

for (let port = from; port <= to; ++port)
	checkPortStatus(port);
