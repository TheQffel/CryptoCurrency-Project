using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OneCoin
{
    class Statistics
    {
        const string Website = "<!-- One Coin Mining Website Statistics -->"
        +"\n<html lang='en'>"
        +"\n<head>"
        +"\n	<meta charset='utf-8'>"
        +"\n	<meta name='viewport' content='width=device-width, initial-scale=1'>"
        +"\n	<title id='sitename'>One Coin Miner Statistics</title>"
        +"\n	<meta name='description' content='One Coin Statistics Page'>"
        +"\n	<meta name='author' content='TheQffel'>"
        +"\n	<style>"
        +"\n	@media all and (min-width: 1px) { body { font-size: 0.1em; } }"
        +"\n	@media all and (min-width: 10px) { body { font-size: 0.2em; } }"
        +"\n	@media all and (min-width: 20px) { body { font-size: 0.3em; } }"
        +"\n	@media all and (min-width: 30px) { body { font-size: 0.4em; } }"
        +"\n	@media all and (min-width: 40px) { body { font-size: 0.5em; } }"
        +"\n	@media all and (min-width: 50px) { body { font-size: 0.6em; } }"
        +"\n	@media all and (min-width: 60px) { body { font-size: 0.7em; } }"
        +"\n	@media all and (min-width: 70px) { body { font-size: 0.8em; } }"
        +"\n	@media all and (min-width: 80px) { body { font-size: 0.9em; } }"
        +"\n	@media all and (min-width: 90px) { body { font-size: 1.0em; } }"
        +"\n	@media all and (min-width: 100px) { body { font-size: 1.1em; } }"
        +"\n	@media all and (min-width: 200px) { body { font-size: 1.2em; } }"
        +"\n	@media all and (min-width: 300px) { body { font-size: 1.3em; } }"
        +"\n	@media all and (min-width: 400px) { body { font-size: 1.4em; } }"
        +"\n	@media all and (min-width: 500px) { body { font-size: 1.5em; } }"
        +"\n	@media all and (min-width: 600px) { body { font-size: 1.6em; } }"
        +"\n	@media all and (min-width: 700px) { body { font-size: 1.7em; } }"
        +"\n	@media all and (min-width: 800px) { body { font-size: 1.8em; } }"
        +"\n	@media all and (min-width: 900px) { body { font-size: 1.9em; } }"
        +"\n	@media all and (min-width: 1000px) { body { font-size: 2.0em; } }"
        +"\n	@media all and (min-width: 1100px) { body { font-size: 2.1em; } }"
        +"\n	@media all and (min-width: 1200px) { body { font-size: 2.2em; } }"
        +"\n	@media all and (min-width: 1300px) { body { font-size: 2.3em; } }"
        +"\n	@media all and (min-width: 1400px) { body { font-size: 2.4em; } }"
        +"\n	@media all and (min-width: 1750px) { body { font-size: 2.5em; } }"
        +"\n	body"
        +"\n	{"
        +"\n		margin: 0px;"
        +"\n		padding: 0px;"
        +"\n		background-color: #2e1f0f;"
        +"\n		font-family: Impact, Charcoal, sans-serif;"
        +"\n		letter-spacing: 1px;"
        +"\n		color: #dddddd;"
        +"\n		transition-property: all;"
        +"\n		transition-duration: 1s;"
        +"\n	}"
        +"\n	#main"
        +"\n	{"
        +"\n		background-color: rgba(0, 0, 0, 0.5);"
        +"\n		margin: auto;"
        +"\n		padding: 5%;"
        +"\n		width: auto;"
        +"\n		max-width: 1750px;"
        +"\n	}"
        +"\n	#body"
        +"\n	{"
        +"\n		clear: both;"
        +"\n		opacity: 0;"
        +"\n		animation: appear 2.5s 2.0s;"
        +"\n		animation-fill-mode: forwards;"
        +"\n	}"
        +"\n	#body p"
        +"\n	{"
        +"\n		clear: both;"
        +"\n		text-align: center;"
        +"\n		font-size: 150%;"
        +"\n	}"
        +"\n	#stats"
        +"\n	{"
        +"\n		float: left;"
        +"\n		width: 50%;"
        +"\n		text-align: center;"
        +"\n	}"
        +"\n	#statistics"
        +"\n	{"
        +"\n		line-height: 150%;"
        +"\n	}"
        +"\n	#statistics span"
        +"\n	{"
        +"\n		float: right;"
        +"\n	}"
        +"\n	#address p"
        +"\n	{"
        +"\n		display: none;"
        +"\n	}"
        +"\n	#header"
        +"\n	{"
        +"\n		overflow: hidden;"
        +"\n		background-color: #333;"
        +"\n		position: fixed;"
        +"\n		top: 0;"
        +"\n		font-size: 75%;"
        +"\n		width: 100%;"
        +"\n	}"
        +"\n	#menu"
        +"\n	{"
        +"\n		margin: auto;"
        +"\n		width: auto;"
        +"\n		max-width: 1750px;"
        +"\n	}"
        +"\n	#menu a"
        +"\n	{"
        +"\n		float: right;"
        +"\n		display: inline-block;"
        +"\n		color: #f2f2f2;"
        +"\n		text-align: center;"
        +"\n		padding: 15px;"
        +"\n		text-decoration: none;"
        +"\n	}"
        +"\n	#menu a:hover"
        +"\n	{"
        +"\n		background: #ddd;"
        +"\n		color: black;"
        +"\n	}"
        +"\n	#menu a#mainbutton"
        +"\n	{"
        +"\n		float: left;"
        +"\n	}"
        +"\n	#footer"
        +"\n	{"
        +"\n		clear: both;"
        +"\n		background-color: #333;"
        +"\n		font-size: 75%;"
        +"\n		margin: auto;"
        +"\n		padding: 1% 5% 1% 5%;"
        +"\n		width: auto;"
        +"\n		max-width: 1750px;"
        +"\n	}"
        +"\n	@media all and (max-width: 800px) { #menu a#mainbutton { display: none; } }"
        +"\n	@media all and (max-width: 700px) { #address p { display: inline; } }"
        +"\n	</style>"
        +"\n	<script src='https://cdn.jsdelivr.net/npm/chart.js'></script>"
        +"\n	<script>"
        +"\n	function GenerateChart()"
        +"\n	{"
        +"\n		Chart.defaults.font.size = parseInt(window.getComputedStyle(document.body, null).getPropertyValue('font-size'))*0.5;"
        +"\n		"
        +"\n		var HashrateChart = new Chart(document.getElementById('hashratechart').getContext('2d'),"
        +"\n		{"
        +"\n			type: 'line',"
        +"\n			data:"
        +"\n			{"
        +"\n				labels: MINERS+DATA-LABELS,"
        +"\n				datasets:"
        +"\n				[{"
        +"\n					label: '   Hashrate  (KH/s)   ',"
        +"\n					data: MINERS+DATA-HASHRATE,"
        +"\n					backgroundColor: ['rgba(250, 150, 50, 1)'],"
        +"\n					borderColor: ['rgba(250, 150, 50, 1)'],"
        +"\n					borderWidth: 5"
        +"\n				},{"
        +"\n					label: '  Solutions   ',"
        +"\n					data: MINERS+DATA-SOLUTIONS,"
        +"\n					backgroundColor: ['rgba(50, 250, 150, 1)'],"
        +"\n					borderColor: ['rgba(50, 250, 150, 1)'],"
        +"\n					borderWidth: 5,"
        +"\n					type: 'bar'"
        +"\n				}],"
        +"\n			}"
        +"\n		});"
        +"\n		HashrateChart.options.plugins.legend.align = 'start';"
        +"\n		HashrateChart.options.plugins.legend.position = 'bottom';"
        +"\n		HashrateChart.options.plugins.legend.labels.usePointStyle = true;"
        +"\n		HashrateChart.update();"
        +"\n	}"
        +"\n    setTimeout(() => { location.reload(true); }, 1000000);"
        +"\n	</script>"
        +"\n</head>"
        +"\n<body onload='GenerateChart();'>"
        +"\n	<div id='main'>"
        +"\n		<div id='content'>"
        +"\n			<div id='address'>"
        +"\n				<p>We highly recommend to switch to desktop mode to view mining statistics.</p>"
        +"\n			</div>"
        +"\n			<div id='statistics'>"
        +"\n				Hashrate And Solutions Chart: <br> <br>"
        +"\n				<canvas id='hashratechart' height='100'></canvas>"
        +"\n				<span id='refreshinfo'> * This statistics will refresh every 15 minutes. </span> <br> <br>"
        +"\n				<br> Your mining address: <span> MINERS+DATA-ADDRESS </span>"
        +"\n				<br> Your current balance: <span> MINERS+DATA-BALANCE </span>"
        +"\n				<br> Your profit per 24h: <span> MINERS+DATA-PROFIT </span>"
        +"\n				<br> Average hashrate: <span> MINERS+DATA-AVGHASH </span>"
        +"\n				<br> Average Solutions: <span> MINERS+DATA-AVGSOLV </span>"
        +"\n			</div>"
        +"\n		</div>"
        +"\n	</div>"
        +"\n	<div id='header'>"
        +"\n		<div id='menu'>"
        +"\n			<a href='.' id='mainbutton'>&nbsp;&nbsp;One Coin Mining Statistics&nbsp;&nbsp;</a>"
        +"\n			<a href='.'>&nbsp;&nbsp;Explorer&nbsp;&nbsp;</a>"
        +"\n			<a href='http://github.com/TheQffel/OneCoin/releases'>&nbsp;&nbsp;Download&nbsp;&nbsp;</a>"
        +"\n			<a href='http://github.com/TheQffel/OneCoin'>&nbsp;&nbsp;Source&nbsp;&nbsp;</a>"
        +"\n		</div>"
        +"\n	</div>"
        +"\n	<div id='footer'>"
        +"\n		<center>One Coin Miner - The Qffel - 2022</center>"
        +"\n	</div>"
        +"\n</body>"
        +"\n</html>";
        
        public static void Update(bool Open = false)
        {
            string WebpageWithStats = Website;
            string[] ToReplace = new string[9];
            
            ToReplace[0] = Directory.GetCurrentDirectory() + "/index.html";
            
            ToReplace[1] = "'NOW'";
            int X = (DateTime.Now.Second + 60 * DateTime.Now.Minute) * 1000;
            ToReplace[2] = "" + (float)Mining.HashrateStats[Mining.CurrentHour]/X;
            ToReplace[3] = "" + (float)Mining.SolutionsStats[Mining.CurrentHour];

            for (int i = Mining.CurrentHour+23; i > Mining.CurrentHour; i--)
            {
                ToReplace[1] = "'" + i%24 + ":00', " + ToReplace[1];
                ToReplace[2] = (float)Mining.HashrateStats[i%24]/3600000 + ", " + ToReplace[2];
                ToReplace[3] = (float)Mining.SolutionsStats[i%24] + ", " + ToReplace[3];
            }
            
            ToReplace[1] = "[" + ToReplace[1] + "]";
            ToReplace[2] = "[" + ToReplace[2] + "]";
            ToReplace[3] = "[" + ToReplace[3] + "]";
            
            ToReplace[4] = Mining.MiningAddress[..10] + " ... " + Mining.MiningAddress[^10..];
            ToReplace[5] = Wallets.GetBalance(Mining.MiningAddress) + "&nbsp; ONES";
            ToReplace[6] = Wallets.MinerRewards[Blockchain.CurrentHeight/1000000]*(ulong)Mining.SolutionsStats.Sum() + "&nbsp; ONES";
            ToReplace[7] = (float)Mining.HashrateStats.Sum()/86400000 + "&nbsp; KH/s";
            ToReplace[8] = (float)Mining.SolutionsStats.Sum()/24 + "&nbsp; SOLV/h";
            
            WebpageWithStats = WebpageWithStats.Replace("MINERS+DATA-LABELS", ToReplace[1]);
            WebpageWithStats = WebpageWithStats.Replace("MINERS+DATA-HASHRATE", ToReplace[2]);
            WebpageWithStats = WebpageWithStats.Replace("MINERS+DATA-SOLUTIONS", ToReplace[3]);
            
            WebpageWithStats = WebpageWithStats.Replace("MINERS+DATA-ADDRESS", ToReplace[4]);
            WebpageWithStats = WebpageWithStats.Replace("MINERS+DATA-BALANCE", ToReplace[5]);
            WebpageWithStats = WebpageWithStats.Replace("MINERS+DATA-PROFIT", ToReplace[6]);
            WebpageWithStats = WebpageWithStats.Replace("MINERS+DATA-AVGHASH", ToReplace[7]);
            WebpageWithStats = WebpageWithStats.Replace("MINERS+DATA-AVGSOLV", ToReplace[8]);
            
            File.WriteAllText(ToReplace[0], WebpageWithStats);
            
            if(Open)
            {
                Process.Start(ToReplace[0]);
            }
        }
    }
}
