const connection = new signalR.HubConnectionBuilder()
    .withUrl("/visualizerhub")
    .build();

var numToDraw = 32; // Note: needs to be a power of 2
var pause = false;
var fftResultGlobal;
var readRateGlobal;
var frames = 1;
var frameNum = 0;

// This pseudo-object is possibly the most important in the file. It contains 
// header information, all data points, a function to return all points (getLog)
// and functions to get the top Ten or top 100 points so far. Archer has mentioned
// storing this in a database instead, which would help reduce space concerns.
var data = {
    header: [],
    points: [],
    getLog: function () {
        return this.points;
    },
    getTopTen: function () {
        var tempArray = this.points.slice(0, 10);
        tempArray.unshift(this.header);
        return tempArray;
    },
    getTop: function (x) {
        var tempArray = this.points.slice(0, x);
        tempArray.unshift(this.header);
        return tempArray;
    },
    getTopNums: function (x) {
        var tempArray2 = [];
        this.points.slice(0, x).forEach(function(element) {
            tempArray2.push([element[0], element[1]]);
        });
        return tempArray2;
    }
}

// Called when a new message is received.
// This method accepts an array of strings for header information and an array of strings for inputs.
connection.on("ReceiveInput", (obj) => {
    console.log(obj);
    const Msg = JSON.parse(obj);
    readRateGlobal = Msg.ReadRate;
    fftResultGlobal = Msg.FFTResult;
    var headers = Msg.NewVal.Headers;
    var inputs = Msg.NewVal.Inputs;
    if (data.header === undefined || data.header.length === 0) {
        data.header = headers;
    }
    var messages = [];
    
    inputs.forEach(function (value) {
        value = parseFloat(value);
        messages.push(value);
        
    });
    data.points.unshift(messages);
    data.points = data.points.slice(0, numToDraw);
    if (!pause && frameNum % frames == 0) {
        drawChart();
    }
    frameNum += 1
});

connection.start().catch(err => console.error(err.toString()));

// Helper function to download the log
document.getElementById("frameButton").addEventListener("click", event => {
    console.log(document.getElementById("frameRate").value);
    var framerate = parseInt(document.getElementById("frameRate").value);
    console.log(framerate);
    if (!isNaN(framerate) && framerate != 0) {
        frames = framerate;
    }
    console.log(frames);
});

document.getElementById("pauseButton").addEventListener("click", event => {
    pause = !pause;
});

document.getElementById("displayNum").addEventListener("click",
    event => {
        var display = document.getElementById("displayNum");
        numToDraw = Math.pow(2, parseInt(display.value));
        //console.log(numToDraw);
        //console.log(display.value);
    });

google.charts.load('current', { 'packages': ['corechart'] });
var chart1 = "";
var chart2 = "";
var fft = FFTNayuki(numToDraw);
// This actually draws the chart. Possible parameterization: allow the user to determine
// How many elements to pull out.
function drawChart() {
    var top = data.getTop(numToDraw);
    var dataTable = google.visualization.arrayToDataTable(top); // This takes care of the first chart
    var options = {
        title: 'Timestamp/Value Chart', hAxis: { title: 'Timestamp' }, vAxis: { title: 'Value' }, curveType: 'function', legend: { position: 'bottom' }
    };
    if (chart1 != "") {
        chart1.clearChart();
    }
    else {
        chart1 = new google.visualization.LineChart(document.getElementById('curve_chart'));
    }
    chart1.draw(dataTable, options);

    var topNumbers = data.getTopNums(numToDraw);

    // Format array for graph 
    var result = [];
    for (var idx = 1; idx < fftResultGlobal.length; idx++) {
        result.push([(idx*readRateGlobal / fftResultGlobal.length), fftResultGlobal[idx], 0]);
    }
    // Header
    result.unshift(["Timestamp", "Real"]);
    var dataFFTTable = google.visualization.arrayToDataTable(result);

    /* Draw Charts! */
    
    if (chart2 != "") {
        chart2.clearChart();
    }
    else {
        chart2 = new google.visualization.LineChart(document.getElementById('fft_chart'));
    }
    chart2.draw(dataFFTTable, options2);
    
}
