use ip_bm
select 
	f.ng_form_name, s.form_segment_name, coalesce(tf.field_name, i.button_name) as field_name, 
	i.field_title, i.visible, i.required, i.read_only, 
	coalesce(dd.dictionary_name, dd2.dictionary_name), i.disconnected_formula, i.ordinal, i.Default_Formula
from ng_form as f
inner join ng_form_segment as s on f.ng_form_id = s.ng_form_id
inner join ng_form_field as i on s.ng_form_segment_id = i.form_segment_id
left outer join table_fields as tf on i.field_id = tf.table_fields_id
left outer join data_dictionary as dd on dd.data_dictionary_id = i.disconnected_type
left outer join data_dictionary as dd2 on dd2.data_dictionary_id = tf.dictionary_id
where f.ng_form_name = 'HBIntDivisionOption'
order by f.ng_form_name, s.ordinal, tf.field_name