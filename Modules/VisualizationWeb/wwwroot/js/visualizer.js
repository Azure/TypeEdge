const connection = new signalR.HubConnectionBuilder()
    .withUrl("/visualizerhub")
    .build();
connection.start().catch(err => console.error(err.toString()));

var numToDraw = 32; // Note: needs to be a power of 2
var pause = false;
var frames = 1;
var frameNum = 0;
var chart2 = "";
var charts = {};
google.charts.load('current', { 'packages': ['corechart'] });


// This pseudo-object is possibly the most important in the file. It contains 
// header information, all data points, a function to return all points (getLog)
// and functions to get the top Ten or top 100 points so far. Archer has mentioned
// storing this in a database instead, which would help reduce space concerns.
class Chart {    
    updatePoints(newPoints) {
        if (this.append === true) {
            this.points = newPoints.concat(this.points);
        }
        else {
            this.points = newPoints;
        }
    }
    constructor(chartName, xlabel, ylabel, headers, points, append, fft) {
        
        this.chartName = chartName;
        this.xlabel = xlabel;
        this.ylabel = ylabel;
        this.headers = headers;
        this.points = points;
        this.append = append;
        this.fft = fft;
        this.googleChart = new google.visualization.LineChart(document.getElementById('curve_chart'));
        this.options = {
            title: chartName, hAxis: { title: xlabel }, vAxis: { title: ylabel }, curveType: 'function', legend: { position: 'bottom' }
        };
    }
}

function getTopNums(chart) {
    var tempArray2 = [];
    //chart.points.slice(0, numToDraw).forEach(function (element) {
    //    tempArray2.push([element[0], element[1]]);
    //});
    return tempArray2;
}
function getTop(chart) {
    //chart.points = chart.points.slice(0, numToDraw); // Slice off extra to preserve number in chart
    var tempArray = chart.points.slice(0, numToDraw);
    tempArray.unshift(chart.headers);
    /*console.log(arrays);
    for (var i = 0; i < chart.headers.length; i++) {
        arrays[i] = [chart.headers[i]];
    }
    console.log(arrays);
    for (var i = 0; i < tempArray.length; i++) {
        for (var j = 0; j < arrays.length; j++) {
            arrays[j].push(tempArray[i][j])
        }
    }
    console.log(arrays);
    return arrays;*/
    return tempArray;
}

// Called when a new message is received.
// This method accepts an array of strings for header information and an array of strings for inputs.
connection.on("ReceiveInput", (obj) => {
    const Msg = JSON.parse(obj);
    console.log(Msg);

    // First, check if we have this chart already and it's just an update.
    
    var messageArray = Msg.messages;
    for (var i = 0; i < messageArray.length; i++) {
        var chart = messageArray[i];
        if (!charts.hasOwnProperty(chart.chartName)) {
            // Does not exist, so let's create it.
            charts[chart.chartName] = new Chart(chart.chartName, chart.xlabel, chart.ylabel, chart.headers, chart.points, chart.append, chart.fft);
        }
        else {
            charts[chart.chartName].updatePoints(chart.points);
        }
    }
    
    
    if (!pause && frameNum % frames == 0) { // Draw charts if user wants
        for (var i = 0; i < messageArray.length; i++) {
            drawChart(charts[messageArray[i].chartName]);
        }
    }
    frameNum += 1
});

// Helper function to change the framerate
document.getElementById("frameButton").addEventListener("click", event => {
    var framerate = parseInt(document.getElementById("frameRate").value);
    if (!isNaN(framerate) && framerate != 0) {
        frames = framerate;
    }
});

document.getElementById("pauseButton").addEventListener("click", event => {
    pause = !pause;
});

// Since the number of elements to display must be a power of 2, we use the sliding
// bar to determine the exponent of number to display.
document.getElementById("displayNum").addEventListener("click",
    event => {
        var display = document.getElementById("displayNum");
        numToDraw = Math.pow(2, parseInt(display.value));
    });


// This actually draws the chart. Possible parameterization: allow the user to determine
// How many elements to pull out.
function drawChart(chart) {
    var top = getTop(chart);
    console.log(top);
    var dataTable = google.visualization.arrayToDataTable(top); // This takes care of the first chart
    
    chart.googleChart.draw(dataTable, chart.options);

    var topNumbers = getTopNums(chart, numToDraw);

    // Format array for graph 
    var result = [];
    for (var idx = 1; idx < fftResultGlobal.length; idx++) {
        result.push([(idx*readRateGlobal / fftResultGlobal.length), fftResultGlobal[idx], 0]);
    }
    // Header
    var dataFFTTable = google.visualization.arrayToDataTable(result);

    /* Draw Second Chart */
    if (chart2 != "") {
        chart2.clearChart();
    }
    else {
        chart2 = new google.visualization.LineChart(document.getElementById('fft_chart'));
    }
    chart2.draw(dataFFTTable, options2);
}

