const connection = new signalR.HubConnectionBuilder()
    .withUrl("/visualizerhub")
    .build();

var numToDraw = 32; // Note: needs to be a power of 2
var pause = false;

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
        this.points.slice(0, x).forEach(function (element) {
            tempArray2.push([element[0], element[1]]);
        })
        return tempArray2;
    },
}

// Called when a new message is received.
// This method accepts an array of strings for header information and an array of strings for inputs.
connection.on("ReceiveInput", (obj) => {
    console.log(obj);
    const Msg = JSON.parse(obj);
    headers = Msg.NewVal.Headers;
    inputs = Msg.NewVal.Inputs;
    //FFTResult.points = obj.FFTResult;
    if (data.header === undefined || data.header.length == 0) {
        data.header = headers;
    }
    var messages = [];
    
    inputs.forEach(function (value) {
        value = parseFloat(value);
        messages.push(value);
        
    });
    data.points.unshift(messages);
    data.points = data.points.slice(0, numToDraw);
    if (!pause) {
        drawChart();
    }
});

// There could be new messages here, for when a user wants to change the refresh rate or
// to parametrize the graph.


// Downloads the log of all points to the user.
function downloadLog() {
    let csvContent = "data:text/csv;charset=utf-8,"
    data.points.forEach(function (rowArray) {
        let row = rowArray.join(",");
        csvContent += row + '\r\n';
    });
    var encodedUri = encodeURI(csvContent);
    window.open(encodedUri);
}

connection.start().catch(err => console.error(err.toString()));

// Helper function to download the log
document.getElementById("downloadButton").addEventListener("click", event => {
    downloadLog();
});

document.getElementById("pauseButton").addEventListener("click", event => {
    pause = !pause;
});

document.getElementById("displayNum").addEventListener("click", event => {
    var display = document.getElementById("displayNum")
    numToDraw = Math.pow(2, parseInt(display.value));
    //console.log(numToDraw);
    //console.log(display.value);
})

google.charts.load('current', { 'packages': ['corechart'] });

// This actually draws the chart. Possible parameterization: allow the user to determine
// How many elements to pull out.
function drawChart() {
    var top = data.getTop(numToDraw);
    var dataTable = google.visualization.arrayToDataTable(top); // This takes care of the first chart
    var topNumbers = data.getTopNums(numToDraw);
    var fft = FFTNayuki(numToDraw);
    var options = {
        title: 'Timestamp/Value Chart', hAxis: { title: 'Timestamp' }, vAxis: { title: 'Value' }, curveType: 'function', legend: { position: 'bottom' }
    };
    var chart1 = new google.visualization.LineChart(document.getElementById('curve_chart'));
    chart1.draw(dataTable, options);

    /* Processing for FFT */
    var reals = [];
    var imags = [];

    // Format array correctly and send to FFT. We send 0 for the imaginaries.
    topNumbers.forEach((val, i) => { reals.push(val[1]); imags.push(0); });
    this.forward(reals, imags);

    // Format array for graph 
    var result = [];
    for (var idx = 1; idx < reals.length; idx++) {
        result.push([topNumbers[idx][0], reals[idx], imags[idx]]);
    }
    // Header
    result.unshift(["Timestamp", "Real", "Imag"]);
    var dataFFTTable = google.visualization.arrayToDataTable(result);

    /* Draw Charts! */

    
    var options2 = {
        title: 'FFT Chart', hAxis: { title: 'Frequency' }, vAxis: { title: 'Amplitude' }, curveType: 'function', legend: { position: 'bottom' }
    };

    var chart2 = new google.visualization.LineChart(document.getElementById('fft_chart'));

    chart2.draw(dataFFTTable, options2);

}
/* 
 * Free FFT and convolution (JavaScript)
 * 
 * Copyright (c) 2014 Project Nayuki
 * Slightly restructured by Chris Cannam, cannam@all-day-breakfast.com
 */

/* 
 * Construct an object for calculating the discrete Fourier transform (DFT) of size n, where n is a power of 2.
 */
function FFTNayuki(n) {

    this.n = n;
    this.levels = -1;

    for (var i = 0; i < 32; i++) {
        if (1 << i === n) {
            this.levels = i;  // Equal to log2(n)
        }
    }
    if (this.levels === -1) {
        throw "Length is not a power of 2";
    }

    this.cosTable = new Array(n / 2);
    this.sinTable = new Array(n / 2);
    for (i = 0; i < n / 2; i++) {
        this.cosTable[i] = Math.cos(2 * Math.PI * i / n);
        this.sinTable[i] = Math.sin(2 * Math.PI * i / n);
    }

    /* 
     * Computes the discrete Fourier transform (DFT) of the given complex vector, storing the result back into the vector.
     * The vector's length must be equal to the size n that was passed to the object constructor, and this must be a power of 2. Uses the Cooley-Tukey decimation-in-time radix-2 algorithm.
     */
    this.forward = function (real, imag) {

        var n = this.n;

        // Bit-reversed addressing permutation
        for (var i = 0; i < n; i++) {
            var j = reverseBits(i, this.levels);
            if (j > i) {
                var temp = real[i];
                real[i] = real[j];
                real[j] = temp;
                temp = imag[i];
                imag[i] = imag[j];
                imag[j] = temp;
            }
        }

        // Cooley-Tukey decimation-in-time radix-2 FFT
        for (var size = 2; size <= n; size *= 2) {
            var halfsize = size / 2;
            var tablestep = n / size;
            for (i = 0; i < n; i += size) {
                for (j = i, k = 0; j < i + halfsize; j++ , k += tablestep) {
                    var tpre = real[j + halfsize] * this.cosTable[k] +
                        imag[j + halfsize] * this.sinTable[k];
                    var tpim = -real[j + halfsize] * this.sinTable[k] +
                        imag[j + halfsize] * this.cosTable[k];
                    real[j + halfsize] = real[j] - tpre;
                    imag[j + halfsize] = imag[j] - tpim;
                    real[j] += tpre;
                    imag[j] += tpim;
                }
            }
        }

        // Returns the integer whose value is the reverse of the lowest 'bits' bits of the integer 'x'.
        function reverseBits(x, bits) {
            var y = 0;
            for (var i = 0; i < bits; i++) {
                y = (y << 1) | (x & 1);
                x >>>= 1;
            }
            return y;
        }
    }

    /* 
     * Computes the inverse discrete Fourier transform (IDFT) of the given complex vector, storing the result back into the vector.
     * The vector's length must be equal to the size n that was passed to the object constructor, and this must be a power of 2. This is a wrapper function. This transform does not perform scaling, so the inverse is not a true inverse.
     */
    this.inverse = function (real, imag) {
        forward(imag, real);
    }
}

