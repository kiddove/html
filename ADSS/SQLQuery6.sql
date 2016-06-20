select * from tb_user_info where id >452 order by id desc

select 1, 2, COUNT(*) from tb_user_info

select * from tb_page_visit_info_xango order by id desc

--update tb_page_visit_info_xango set page = 'stat/sample/sample4.html' where id >= 440


--insert into tb_page_visit_info_xango (token, ip, agent, language, color_depth, screen_resolution, time_zone, platform, device, os, register_time, visit_time, country, province, city, province_code)
--select token, ip, agent, language, color_depth, screen_resolution, time_zone, platform, device, os, register_time, activate_time, country, province, city, province_code from tb_user_info where id > 455


--declare @alias varchar(64) = null

--set @alias = (select top 1 alias from tb_page_visit_info_xango where id = 439)
--if (@alias is null)
--print 'null'
--else 
--print @alias

--select * from tb_page_visit_info_xango where token = '949f7ba7f862fd334ac38a1e3c59d1f7' order by visit_time desc

--update tb_page_visit_info_xango set distributor = 'paul' where token = '949f7ba7f862fd334ac38a1e3c59d1f7'


select 'yesterday' as period, type, count(type) as count from tb_page_visit_info_xango where convert(date, visit_time) >= '2016-06-07' and convert(date, visit_time) <= '2016-06-07' group by type;

select convert(date, visit_time)


select  convert(date, visit_time) from tb_page_visit_info_xango order by visit_time desc



select t3.d, DATEDIFF(SECOND,{d '1970-01-01'}, t3.d), t3.t, COUNT(t4.type)as c from tb_page_visit_info_xango t4 right join (select * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango  where distributor='paul' and visit_time >= convert(datetime,'2016-06-07') and visit_time < convert(datetime,'2016-06-08')) t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 ) t3 on CAST(t4.visit_time as DATE) = t3.d and t4.type = t3.t group by t3.d, t3.t order by t3.d, t3.t

select refer from tb_page_visit_info_xango where distributor = 'kectech'


select top 5 refer, COUNT(*) as visit, COUNT(*) * 1.0/ SUM(COUNT(*)) over() as percentage from tb_page_visit_info_xango where distributor='paul' and convert(date, visit_time) >= '2016-05-09' and convert(date, visit_time) <= '2016-06-07' group by refer order by percentage desc

--update tb_page_visit_info_xango set refer = '' where refer is null


select t3.d, t3.t, COUNT(t4.type)as c from tb_page_visit_info_xango t4 
right join (select * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango  where distributor='kectech' and convert(date, visit_time) >= '2016-05-10' and convert(date, visit_time) <= '2016-06-08') t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 ) t3 
on CAST(t4.visit_time as DATE) = t3.d and t4.type = t3.t and t4.distributor = 'kectech' group by t3.d, t3.t order by t3.d, t3.t



select * from (select distinct CAST(visit_time as DATE) as d from tb_page_visit_info_xango  where distributor='kectech' and convert(date, visit_time) >= '2016-05-10' and convert(date, visit_time) <= '2016-06-08') t1 ,(select distinct type as t from tb_page_visit_info_xango) t2 


select distinct(province_code) from tb_page_visit_info_xango  order by visit_time desc

select province, city from tb_page_visit_info_xango where province_code = '35'


select 'yesterday' as period, type, count(type) as count from tb_page_visit_info_xango where distributor = 'kectech' and convert(date, visit_time) >= '2016-06-13' and convert(date, visit_time) <= '2016-06-13' group by type;


select * from tb_page_visit_info_xango where distributor in ('kectech', 'healthylife') order by visit_time desc

select * from tb_page_ads_click_stat



select token, distributor from tb_page_visit_info_xango group by token, distributor order by token

--update tb_page_visit_info_xango set uniq = 'kectech' where distributor = 'healthylife'


select distributor, uniq from tb_page_visit_info_xango where distributor <> uniq


select * from tb_page_visit_info_xango where distributor = 'healthylife'

select * from tb_page_visit_info_xango 
--update tb_page_visit_info_xango set uniq = distributor where uniq is null

--update tb_page_visit_info_xango set alias = (select alias from tb_page_visit_info_xango where )


select * from tb_distributor_blog

insert into tb_distributor_blog (token,uniq, alias )
select token, uniq, token from tb_page_visit_info_xango group by token, uniq



select b.alias, a.ip, a.visit_time, a.type, a.page, a.refer, a.country, a.province, a.city, a.province_code from tb_page_visit_info_xango as a , tb_distributor_blog as b 
where a.uniq = b.uniq and a.uniq='kectech'

insert into tb_blog_to_distributor values ('healthylife', 'kectech');

--truncate table tb_blog_to_distributor
select * from tb_blog_to_distributor


select top 20 token, alias, visit_time, distributor, page from tb_page_visit_info_xango order by visit_time desc


select token, alias from tb_page_visit_info_xango where distributor in ('kectech', 'healthylife') and token = 'ae5f55e6b88f6b7ec7195fb4d9e304c0'


select url, page, click_time, distributor from tb_page_ads_click_stat order by click_time desc

select * from tb_page_visit_info_xango where distributor = 'kectech' and alias = 'who is it' and type = 'new' order by visit_time desc


select token, alias, type from tb_page_visit_info_xango where visit_time > '2016-06-10' and type = 'new' and distributor = 'kectech' group by token, alias, type

select id, visit_time from tb_page_visit_info_xango where visit_time = '2016-06-17 13:46:01'

select * from tb_page_visit_info_xango where id > 1062


select * from tb_page_visit_info_xango where distributor = 'healthylife' and type = 'new'


select distinct token, alias from tb_page_visit_info_xango where distributor = 'kectech'

select distinct token, alias from tb_page_visit_info_xango where distributor = 'kectech'

select id from tb_page_visit_info_xango where distributor = 'kectech' and type = 'new' and token = '16bacab05c412e7938475ffa159c539e'


select id from tb_page_visit_info_xango where distributor = 'kectech' and type = 'new' and token = '64eccd20cb270fa9a54d6ddc00ec09b8'




select url, page, COUNT(*) as c from tb_page_ads_click_stat 
where distributor = 'kectech'
group by distributor, url, page
order by c desc