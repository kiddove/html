select distinct id, token, action, time, ip, valid from tb_user_stat where valid = 1 order by time desc

select COUNT(*) from tb_user_stat where valid = 1

select * from tb_user_stat where valid = 1 group by token, action

select (tb_user_info.country + tb_user_info.province_code + tb_user_info.city) as code, tb_user_info.country, tb_user_info.province, tb_user_info.province_code, tb_user_info.city, tb_user_stat.action, COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) as ct from tb_user_info inner join tb_user_stat on tb_user_info.token = tb_user_stat.token and tb_user_info.ip = tb_user_stat.ip and tb_user_stat.id = 126 and tb_user_stat.valid = 1 group by tb_user_info.country, tb_user_info.province, tb_user_info.province_code, tb_user_info.city, tb_user_stat.action having COUNT(distinct tb_user_stat.token + tb_user_stat.ip  + CAST(tb_user_stat.action as varchar(10))) > 0 order by tb_user_info.country, tb_user_info.province, tb_user_info.province_code, tb_user_info.city

select s.id, s.token, s.action, cast(DATEDIFF(SECOND,{d '1970-01-01'}, s.time) as bigint) *1000 as time, s.ip from tb_user_info as i, tb_user_stat as s where i.token = s.token and s.valid = 1 and i.ip = s.ip and s.token = '0118d30b296fcf21cab1d247b00464a3' order by s.id, s.time

select * from tb_ads_info where type_ads in (3, 4)


select distinct(id) from dbo.tb_ads_info where camp_status = 1 and type_ads in (1, 2, 3) and camp_start_date <= GETDATE() and GETDATE() <= camp_stop_date;


select distinct( i.token), i.ip, i.agent, i.language, i.color_depth, i.screen_resolution, i.time_zone, i.platform, i.device, i.os, i.country, i.province, i.city from tb_user_info as i, tb_user_stat as s where i.token = s.token and s.valid = 1 and i.ip = s.ip


update tb_page_visit_info_xango set type ='return' where type is null
update tb_page_visit_info_xango set distributor = 'paul'
select COUNT(*) from tb_user_info
update tb_page_visit_info_xango set alias = token
select id, token, ip, language, device, visit_time, country, province, city from tb_page_visit_info_xango

select distinct token, alias, COUNT(token)as count, country, province, city, language, device from tb_page_visit_info_xango where distributor ='PAUL' and visit_time > '2015-11-23' group by token, alias, country, province, city, language, device having COUNT(token) > 1

select * from tb_page_visit_info_xango where token = 'f23f21ec80378b95be3c4c953d680b34' and visit_time > '2015-11-23 10:32:39.000' order by visit_time

--truncate table tb_page_visit_info_xango

--update tb_page_visit_info_xango set refer = 'www.wyslink.com' where id % 4 = 0
--update tb_page_visit_info_xango set refer = 'www.google.com' where id % 4 = 1
--update tb_page_visit_info_xango set refer = 'blog.wyslink.com' where id % 4 = 2
--update tb_page_visit_info_xango set refer = null where id % 4 = 3

--update tb_page_visit_info_xango set page = 'stat/sample/sample1.html' where id %3 = 0
--update tb_page_visit_info_xango set page = 'stat/sample/sample2.html' where id %3 = 1
--update tb_page_visit_info_xango set page = 'stat/sample/sample3.html' where id %3 = 2

--insert into tb_page_visit_info_xango (token, ip, agent, language, color_depth, screen_resolution, time_zone, platform, device, os, register_time, visit_time, country, province, city, province_code)
--select token, ip, agent, language, color_depth, screen_resolution, time_zone, platform, device, os, register_time, activate_time, country, province, city, province_code from tb_user_info

select distinct token, alias, COUNT(token)as count, country, province, city, language, device from (select * from tb_page_visit_info_xango where visit_time > '2015-11-23 10:32:39.000' ) as t group by token, alias, country, province, city, language, device having COUNT(token) > 1


select * from tb_user_info

select distinct alias, COUNT(alias)as count, language, device from tb_page_visit_info_xango where visit_time >= '2014-04-28' and visit_time <= '2016-05-27' and distributor = 'paul' group by alias, language, device

select distinct token, alias, COUNT(token)as count, language, device from tb_page_visit_info_xango where distributor = 'paul' group by token, alias, language, device having COUNT(token) > 1


select * from tb_page_visit_info_xango where token='02975933b96bcc0b3f36e6d4a2ab6655'


select distinct token, alias, ip, country, province, city, language, device from tb_page_visit_info_xango


select ip, country, province, city, DATEDIFF(SECOND,{d '1970-01-01'}, visit_time) as time from tb_page_visit_info_xango where distributor = 'paul' and token='02975933b96bcc0b3f36e6d4a2ab6655' and visit_time >= '2016-04-28' and visit_time <= '2016-05-27' order by visit_time desc


select * from tb_page_visit_info_xango where alias = 'test' and distributor='paul'
select * from tb_page_visit_info_xango where alias = 'test' and distributor='paul'

select * from tb_page_visit_info_xango order by token




select 'yesterday' as period, COUNT(type) as visit from tb_page_visit_info_xango where visit_time > '2015-09-09' and visit_time < '2016-09-09' group by type
select 'yesterday', COUNT(id) from tb_page_visit_info_xango where visit_time > '2015-09-09' and visit_time < '2016-09-09' and TYPE = 'return'
select COUNT(*) from tb_page_visit_info_xango where visit_time > '2014-09-09' and visit_time < '2015-09-09' union
select COUNT(*) from tb_page_visit_info_xango where visit_time > '2013-09-09' and visit_time < '2014-09-09'


select * from tb_page_visit_info_xango where type = 'new'




select 'Yesterday' as period, type, count(type) as count from tb_page_visit_info_xango where visit_time >= '2016-06-01' and visit_time <= '2016-06-02' group by type;select 'Last 7 days' as period, type, count(type) as count from tb_page_visit_info_xango where visit_time >= '2016-05-27' and visit_time <= '2016-06-02' group by type;select 'Last 30 days' as period, type, count(type) as count from tb_page_visit_info_xango where visit_time >= '2016-05-04' and visit_time <= '2016-06-02' group by type;select 'This month' as period, type, count(type) as count from tb_page_visit_info_xango where visit_time >= '2016-06-01' and visit_time <= '2016-06-30' group by type;select 'Last month' as period, type, count(type) as count from tb_page_visit_info_xango where visit_time >= '2016-05-01' and visit_time <= '2016-05-31' group by type;

select alias, ip, visit_time, type, page, refer, country, province, city from tb_page_visit_info_xango where visit_time >= '2014-06-01' and visit_time <= '2016-06-02' and distributor = 'paul'


update tb_page_visit_info_xango set alias = token where alias = 'test'

select * from tb_page_visit_info_xango where province like 'T%'

update tb_page_visit_info_xango set Province = 'Tokyo' where id in (310, 420)


select YEAR(visit_time), MONTH(visit_time), DAY(visit_time), COUNT(id) from tb_page_visit_info_xango where visit_time <= '2016-05-05' group by YEAR(visit_time), MONTH(visit_time), DAY(visit_time)


select CONVERT(char(8), visit_time, 112), COUNT(id) from tb_page_visit_info_xango where visit_time <= '2016-05-05' and type = 'new' group by CONVERT(char(8), visit_time, 112)
select CAST(visit_time as DATE) as d, type, COUNT(type) from tb_page_visit_info_xango where visit_time <= '2016-05-05' group by CAST(visit_time as DATE), type order by CAST(visit_time as DATE)


select CAST(visit_time as DATE), COUNT(type) from tb_page_visit_info_xango where visit_time <= '2016-05-05' and type = 'new' group by CAST(visit_time as DATE) order by CAST(visit_time as DATE)
select CAST(visit_time as DATE), COUNT(type) from tb_page_visit_info_xango where visit_time <= '2016-05-05' and type = 'return' group by CAST(visit_time as DATE) order by CAST(visit_time as DATE)
select * from page_visit_info_xango_date_view

-- create view page_visit_info_xango_date_view as select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango

-- create view page_visit_info_xango_type_view as select distinct type as t from tb_page_visit_info_xango

select tb_page_visit_info_xango.type, COUNT(tb_page_visit_info_xango.type) from tb_page_visit_info_xango
left join (select 'new' as type union all select 'return' union all select 'test') all_type
on tb_page_visit_info_xango.type = all_type.type


select CAST(t1.visit_time as DATE), COUNT(t1.type)from tb_page_visit_info_xango t1 
left join (
select * from page_visit_info_xango_date_view ,page_visit_info_xango_type_view order by page_visit_info_xango_date_view.d, page_visit_info_xango_type_view.t
) t2

on CAST(t1.visit_time as DATE) = t2.d and t1.type = t2.type

--create view page_visit_info_xango_date_type_list_view as select * from page_visit_info_xango_date_view ,page_visit_info_xango_type_view order by page_visit_info_xango_date_view.d, page_visit_info_xango_type_view.t
select * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango where visit_time < '2016-05-05') t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 order by t1.d, t2.t

select t3.d, t3.t, COUNT(t4.type)as c from tb_page_visit_info_xango t4 right join (select * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango  where visit_time < '2016-01-01') t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 ) t3 on CAST(t4.visit_time as DATE) = t3.d and t4.type = t3.t group by t3.d, t3.t order by t3.d, t3.t


select DATEDIFF(SECOND,{d '1970-01-01'}, t3.d), t3.d, t3.t, COUNT(t4.type)as c 
from tb_page_visit_info_xango t4 
right join 
(select * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango  where distributor='paul' and visit_time >= '2016-05-05' and visit_time <= '2016-06-03') t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 ) t3 
on CAST(t4.visit_time as DATE) = t3.d and t4.type = t3.t 
group by t3.d, t3.t 
order by t3.d, t3.t

DATEDIFF(SECOND,{d '1970-01-01'}, visit_time)


select distinct(page), COUNT(id) as visit from tb_page_visit_info_xango, (select COUNT(*) from tb_page_visit_info_xango) t2 where distributor='paul' group by page order by visit desc


select ROUND(1 /cast((select COUNT(*) from tb_page_visit_info_xango) as decimal(18,2)) * 100.0, 3) as percentage

select page, COUNT(*) as count, COUNT(*) * 100.0 / SUM(COUNT(*)) over() as percentage from tb_page_visit_info_xango group by page order by percentage desc


select top 5 page, COUNT(*) as count, COUNT(*) / SUM(COUNT(*)) over() as percentage from tb_page_visit_info_xango where distributor='paul' and visit_time >= '2016-05-05' and visit_time <= '2016-06-03' group by page order by percentage desc



select type, visit_time from tb_page_visit_info_xango order by visit_time desc





insert into tb_page_visit_info_xango (token, ip, agent, language, color_depth, screen_resolution, time_zone, platform, device, os, country, province, city, province_code, refer, page, distributor) values ('949f7ba7f862fd334ac38a1e3c59d1f7', '174.117.34.102', 'Chrome 51', 'en-US', 24, '1080x1920', 240, 'Win32', 'Desktop', 'Windows 7', 'Canada', 'Ontario', 'Toronto', 'ON', 'http://206.190.141.88/ShowRoomMF.aspx', '/BusinessShowRoom.aspx', 'kectech')
