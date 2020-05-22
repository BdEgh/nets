let XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;
let args = process.argv.slice(2);

if (args[0] == undefined || args[1] == undefined) {
	console.log("Wrong arguments");
	process.exit(9);
}

let userid = args[0];
let key = args[1];

const request = new XMLHttpRequest();
const url = "https://api.vk.com/method/friends.get?user_id=" + userid +"&fields=nickname&access_token=" + key + "&v=5.52";
request.open('GET', url);

request.addEventListener("readystatechange", () => {
	if (request.readyState === 4 && request.status === 200) {
		data = JSON.parse(request.responseText);
		if (data.response == undefined)
			console.log(data.error.error_msg);
		else {
			console.log("User have " + data.response.count + " friends");
			for (i in data.response.items)
				console.log(data.response.items[i].first_name + ' ' + data.response.items[i].last_name);
		}
	}
});
request.send();
