<?php
$return = array ();

if (isset ( $_GET ['c'] )) {
	// $channel = "tony_kectech_com_ch3";
	$channel = $_GET ['c'];
	$filename = '../ccImage/' . $channel . '/index.db';
	if (file_exists ( $filename )) {
		$db = new SQLite3 ( $filename );
		$results = null;
		if (isset ( $_GET ['t'] ) && isset ( $_GET ['d'] ) && isset ( $_GET ['gt'] )) {
			// time
			// date
			$time = $_GET ['t'];
			$date = $_GET ['d'];
			$gt = $_GET ['gt'];
			$sql = null;
			if ($gt < 0) {
				$sql = "select date, time from imageindex where type = '' and date <= " . $date . " and time <= " . $time . " order by date desc, time desc LIMIT 1";
			} else
				// select date, time from imageindex where type = '' and date <= 20160608 and time <= 130101 order by date desc, time desc LIMIT 1
				$sql = "select date, time from imageindex where type = '' and date >= " . $date . " and time >= " . $time . " order by date, time LIMIT 1";
			
			$results = $db->query ( $sql );
		} else {
			$results = $db->query ( "select date, time from imageindex where type = '' order by date desc, time desc LIMIT 1" );
		}
		
		while ( $row = $results->fetchArray () ) {
			$return ["path"] = "http://206.190.133.140/ccImage/" . $channel . "/" . $row ["date"] . "/" . $row ["date"] . sprintf ( "%'.06d", $row ["time"] ) . ".jpg";
			$return ["dt"] = $row ["date"] . sprintf ( "%'.06d", $row ["time"] );
		}
		
		$db->close ();
	}
}

echo json_encode ( $return );
