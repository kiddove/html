select distinct a.token, a.os, a.device, a.country, a.province, a.city, a.province_code, datename(dw, b.time) as dow, YEAR(b.time) as year, MONTH(b.time) as month from tb_user_info as a inner join tb_user_stat as b
on b.id = 126 and a.token = b.token and a.ip = b.ip order by b.time


select distinct a.device, a.country, a.province, a.city, a.province_code from tb_user_info as a inner join tb_user_stat as b
on b.id = 126 and a.token = b.token and a.ip = b.ip


select * from tb_bid_info where id = 126


-- step1 get location
select distinct a.country, a.province, a.city, a.province_code, b.valid from tb_user_info as a inner join tb_user_stat as b
on b.id in (135, 136, 137) and a.token = b.token and a.ip = b.ip and b.valid > 0


-- get list of one locatioon, the question is how to get the valid ones...
-- uset a insert trigger to set valid, invalid, assume when you select for result, there are some valid records.
select 
count(distinct b.token + b.ip + CAST(b.action as varchar(10)))
--, YEAR(b.time) as year, MONTH(b.time) as month
--,DAY(b.time) as day 
from tb_user_info as a inner join tb_user_stat as b on b.id = 126 and a.token = b.token and a.ip = b.ip 
and a.country = 'canada' and a.province_code = 'on' and a.city = 'richmond hill'

select count(distinct b.token + b.ip) from tb_user_info as a inner join tb_user_stat as b on b.id = 126 and a.token = b.token and a.ip = b.ip and a.country = 'canada' and a.province_code = 'on' and a.city = 'richmond hill'

-- try using group by having

select tb_ads_info.id, COUNT(distinct tb_user_stat.token + tb_user_stat.ip) as what from
(tb_ads_info inner join tb_user_stat on tb_ads_info.id = tb_user_stat.id)
group by tb_ads_info.id
--having COUNT(distinct tb_user_stat.token + tb_user_stat.ip) > 5

select tb_user_info.*, tb_user_stat.* from tb_user_info inner join tb_user_stat
on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip
and (tb_user_info.province = 'all' or tb_user_info.city = 'all' or tb_user_info.province_code = 'all')


select * from tb_user_stat order by time 



--insert into dbo.tb_user_stat (id, token, action, time, ip) values (126, '9zbad16d2f80e08a8ca63e736389b97e3', 1, '2015-09-21 13:15:03.000', '299.237.172.93')


select top 20 * from tb_user_stat where  id = 126 and ip = '299.237.172.93' order by time desc



select count(distinct b.token + b.ip) from tb_user_info as a inner join tb_user_stat as b on b.id = 137 and a.token = b.token and a.ip = b.ip and a.country = 'Canada' and a.province_code = 'all' and a.city = 'all' and b.action = 2 and b.valid = 1

select * from  tb_bid_info where id = 126


select count(distinct b.token + b.ip) from tb_user_info as a inner join tb_user_stat as b on b.id = 126 and a.token = b.token and a.ip = b.ip and a.country = 'Canada' and a.province_code = 'ON' and a.city = 'Richmond hill' and b.valid = 1


--update tb_user_stat set valid = 1 where id = 126

select * from tb_user_stat where id = 126 and action = 1

--update tb_user_stat set action = 2 where id = 126 and action = 1


--- by single date
select distinct YEAR(tb_user_stat.time) as year, MONTH(tb_user_stat.time) as month ,DAY(tb_user_stat.time) as day from tb_user_info inner join tb_user_stat 
on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip 
and tb_user_stat.id = 126 and tb_user_stat.valid = 1


select tb_user_stat.action, COUNT(*)as ct from tb_user_info inner join tb_user_stat on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip 
and tb_user_stat.id = 126 and tb_user_stat.valid = 1
and YEAR(tb_user_stat.time) = 2015
and MONTH(tb_user_stat.time) = 8
and DAY(tb_user_stat.time) = 11
group by tb_user_stat.action

--- total
select YEAR(tb_user_stat.time), MONTH(tb_user_stat.time), DAY(tb_user_stat.time), COUNT(*)as ct from tb_user_info inner join tb_user_stat on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip 
and tb_user_stat.id = 126 and tb_user_stat.valid = 1
group by YEAR(tb_user_stat.time), MONTH(tb_user_stat.time), DAY(tb_user_stat.time)

--- seems to be correct, count(distinct(c, p, pcode, c, action)), this is a total number, without action
-- stat by location
select (tb_user_info.country + tb_user_info.province_code + tb_user_info.city) as code, tb_user_info.country, tb_user_info.province, tb_user_info.province_code, tb_user_info.city, tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) as ct 
from tb_user_info inner join tb_user_stat 
on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip 
and tb_user_stat.id = 126 and tb_user_stat.valid = 1
group by tb_user_info.country, tb_user_info.province, tb_user_info.province_code, tb_user_info.city, tb_user_stat.action
having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0
order by tb_user_info.country, tb_user_info.province, tb_user_info.province_code, tb_user_info.city


-- stat by date
select (YEAR(tb_user_stat.time) * 10000 + MONTH(tb_user_stat.time) * 100 + DAY(tb_user_stat.time)) as iDate , datename(dw, tb_user_stat.time) as dow, tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) as ct 
from tb_user_info inner join tb_user_stat 
on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip 
and tb_user_stat.id = 126 and tb_user_stat.valid = 1
group by YEAR(tb_user_stat.time), MONTH(tb_user_stat.time), DAY(tb_user_stat.time), datename(dw, tb_user_stat.time), tb_user_stat.action
having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0
order by (YEAR(tb_user_stat.time) * 10000 + MONTH(tb_user_stat.time) * 100 + DAY(tb_user_stat.time))

-- stat by dow
select datepart(dw, tb_user_stat.time) as di, datename(dw, tb_user_stat.time) as dow, tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) as ct 
from tb_user_info inner join tb_user_stat 
on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip 
and tb_user_stat.id = 126 and tb_user_stat.valid = 1
group by datename(dw, tb_user_stat.time), datepart(dw, tb_user_stat.time), tb_user_stat.action
having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0
order by datepart(dw, tb_user_stat.time)

-- stat by week, from Sun. to Sat.
select re.id, (YEAR(re.start_of_week) * 10000 + MONTH(re.start_of_week) * 100 + DAY(re.start_of_week)) as startDate, dbo.fnFormatDate(re.start_of_week, 'Mon DD, YYYY') as ss,
(YEAR(re.end_of_week) * 10000 + MONTH(re.end_of_week) * 100 + DAY(re.end_of_week)) as endDate, dbo.fnFormatDate(re.end_of_week, 'Mon DD, YYYY') as ae,
re.action, re.ct
from(
select YEAR(tb_user_stat.time) * 100 + datepart(wk, tb_user_stat.time) as id, 
DATEADD(wk, DATEDIFF(wk, 6, CAST(RTRIM(YEAR(tb_user_stat.time) * 10000 + 1 * 100 + 1) AS DATETIME)) + ( datepart(wk, tb_user_stat.time) - 1 ), 6) AS [start_of_week],
DATEADD(second, -1, DATEADD(day, DATEDIFF(day, 0, DATEADD(wk, DATEDIFF(wk, 5, CAST(RTRIM(YEAR(tb_user_stat.time) * 10000 + 1 * 100 + 1) AS DATETIME)) + ( datepart(wk, tb_user_stat.time) + -1 ), 5)) + 1, 0)) AS [end_of_week],
tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip + CAST(tb_user_stat.action as varchar(10))) as ct 
from tb_user_info inner join tb_user_stat 
on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip 
and tb_user_stat.id = 126 and tb_user_stat.valid = 1
group by YEAR(tb_user_stat.time), datepart(wk, tb_user_stat.time), tb_user_stat.action
having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0) as re
order by re.id

-- stat by device, desktop mobile tablit
--- stat by pltform , win32 iphone, android, ipad
select tb_user_info.device, tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) as ct 
from tb_user_info inner join tb_user_stat 
on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip 
and tb_user_stat.id = 126 and tb_user_stat.valid = 1
group by tb_user_info.device, tb_user_stat.action
having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0
order by tb_user_info.device




select count(distinct token + ip + CAST(action as varchar(10))) from tb_user_stat
where id = 126 and valid = 1
and YEAR(tb_user_stat.time) = 2015
and MONTH(tb_user_stat.time) = 8
and DAY(tb_user_stat.time) = 11


select distinct(device) from tb_user_info

select distinct(platform) from tb_user_info

select distinct(os) from tb_user_info


select DATEPART(WK, getdate())

-- year 20015 week 33
DECLARE @WeekNum INT = 62,
    @YearNum INT = 2015 ;

SELECT  DATEADD(wk, DATEDIFF(wk, 6, CAST(RTRIM(@YearNum * 10000 + 1 * 100 + 1) AS DATETIME)) + ( @WeekNum - 1 ), 6) AS [start_of_week],
                
        DATEADD(second, -1, DATEADD(day, DATEDIFF(day, 0, DATEADD(wk, DATEDIFF(wk, 5, CAST(RTRIM(@YearNum * 10000 + 1 * 100 + 1) AS DATETIME)) + ( @WeekNum + -1 ), 5)) + 1, 0)) AS [end_of_week] ;