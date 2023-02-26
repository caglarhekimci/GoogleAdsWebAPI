using Google.Ads.GoogleAds.V12.Common;
using System.Reflection;
using System.Text;

namespace GoogleAdsAPI.Utilities.Helpers
{
    public static class Builder
    {
        public static string BuildQuery(object obj, string tableName,long? campaignId)
        {
            var adGroupSelectableList = new List<string> { "id", "name", "status", "campaign_id", "cpc_bid_micros", "cpm_bid_micros", "cpv_bid_micros",
                                                        "target_cpa_micros", "target_roas", "target_spend_micros", "percent_cpc_bid_micros", "search_budget_micros", 
                                                        "display_budget_micros", "shopping_setting_id", "campaign_criterion_id", "ad_group_criterion_id", 
                                                        "tracking_url_template", "final_url_suffix", "url_custom_parameters", "ad_rotation_mode", "labels", 
                                                        "base_ad_group_id", "ad_group_type", "ad_group_experiment_data", "ad_group_audience_criteria_id", 
                                                        "ad_group_bid_modifiers", "ad_group_negative_keywords", "ad_group_negative_keyword_lists", 
                                                        "ad_group_dynamic_search_ads_setting", "adgroup_hotel_setting", "ad_group_local_service_setting", 
                                                        "ad_group_local_ad_info", "ad_group_shopping_setting", "ad_group_smart_shopping_setting", "ad_group_product_bidding_category",
                                                        "ad_group_promotion", "ad_group_similar_remarketing_list", "ad_group_size", "ad_group_tracking_url_template", "ad_group_url_custom_parameters" };

            var campaignSelectableList = new List<string> { "campaign.accessible_bidding_strategy", "campaign.ad_serving_optimization_status", "campaign.advertising_channel_sub_type",
                                                            "campaign.advertising_channel_type", "campaign.app_campaign_setting.app_id", "campaign.app_campaign_setting.app_store", "campaign.app_campaign_setting.bidding_strategy_goal_type",
                                                            "campaign.audience_setting.use_audience_grouped", "campaign.base_campaign", "campaign.bidding_strategy", "campaign.bidding_strategy_system_status", "campaign.bidding_strategy_type",
                                                            "campaign.campaign_budget","campaign.campaign_group","campaign.commission.commission_rate_micros","campaign.dynamic_search_ads_setting.domain_name", "campaign.dynamic_search_ads_setting.feeds",
                                                            "campaign.dynamic_search_ads_setting.language_code", "campaign.dynamic_search_ads_setting.use_supplied_urls_only", "campaign.end_date", "campaign.excluded_parent_asset_field_types",
                                                            "campaign.excluded_parent_asset_set_types", "campaign.experiment_type", "campaign.final_url_suffix", "campaign.frequency_caps", "campaign.geo_target_type_setting.negative_geo_target_type",
                                                            "campaign.geo_target_type_setting.positive_geo_target_type", "campaign.hotel_setting.hotel_center_id", "campaign.id", "campaign.labels", "campaign.local_campaign_setting.location_source_type",
                                                            "campaign.local_services_campaign_settings.category_bids", "campaign.manual_cpa", "campaign.manual_cpc.enhanced_cpc_enabled", "campaign.manual_cpm", "campaign.manual_cpv",
                                                            "campaign.maximize_conversion_value.target_roas", "campaign.maximize_conversions.target_cpa_micros", "campaign.name", "campaign.network_settings.target_content_network",
                                                            "campaign.network_settings.target_google_search", "campaign.network_settings.target_partner_search_network", "campaign.network_settings.target_search_network",
                                                            "campaign.optimization_goal_setting.optimization_goal_types", "campaign.optimization_score", "campaign.payment_mode", "campaign.percent_cpc.cpc_bid_ceiling_micros",
                                                            "campaign.percent_cpc.enhanced_cpc_enabled", "campaign.performance_max_upgrade.performance_max_campaign", "campaign.performance_max_upgrade.pre_upgrade_campaign",
                                                            "campaign.performance_max_upgrade.status", "campaign.primary_status", "campaign.primary_status_reasons", "campaign.real_time_bidding_setting.opt_in", "campaign.resource_name",
                                                            "campaign.selective_optimization.conversion_actions", "campaign.serving_status", "campaign.shopping_setting.campaign_priority", "campaign.shopping_setting.enable_local",
                                                            "campaign.shopping_setting.feed_label", "campaign.shopping_setting.merchant_id", "campaign.shopping_setting.sales_country", "campaign.shopping_setting.use_vehicle_inventory",
                                                            "campaign.start_date", "campaign.target_cpa.cpc_bid_ceiling_micros", "campaign.target_cpa.cpc_bid_floor_micros", "campaign.target_cpa.target_cpa_micros", "campaign.target_cpm",
                                                            "campaign.target_impression_share.cpc_bid_ceiling_micros", "campaign.target_impression_share.location", "campaign.target_impression_share.location_fraction_micros",
                                                            "campaign.target_roas.cpc_bid_ceiling_micros", "campaign.target_roas.cpc_bid_floor_micros", "campaign.target_roas.target_roas", "campaign.target_spend.cpc_bid_ceiling_micros",
                                                            "campaign.target_spend.target_spend_micros", "campaign.targeting_setting.target_restrictions", "campaign.tracking_setting.tracking_url", "campaign.tracking_url_template",
                                                            "campaign.url_custom_parameters", "campaign.url_expansion_opt_out", "campaign.vanity_pharma.vanity_pharma_display_url_mode", "campaign.vanity_pharma.vanity_pharma_text",
                                                            "campaign.video_brand_safety_suitability" };
            var metricsForCampaign = new List<string> { "metrics.absolute_top_impression_percentage", "metrics.active_view_cpm", "metrics.active_view_ctr", "metrics.active_view_impressions", "metrics.active_view_measurability",
                "metrics.active_view_measurable_cost_micros", "metrics.active_view_measurable_impressions", "metrics.active_view_viewability", "metrics.all_conversions",
                "metrics.all_conversions_by_conversion_date", "metrics.all_conversions_from_click_to_call", "metrics.all_conversions_from_directions", "metrics.all_conversions_from_interactions_rate",
                "metrics.all_conversions_from_location_asset_click_to_call", "metrics.all_conversions_from_location_asset_directions", "metrics.all_conversions_from_location_asset_menu", 
                "metrics.all_conversions_from_location_asset_order", "metrics.all_conversions_from_location_asset_other_engagement", "metrics.all_conversions_from_location_asset_store_visits",
                "metrics.all_conversions_from_location_asset_website", "metrics.all_conversions_from_menu", "metrics.all_conversions_from_order", "metrics.all_conversions_from_other_engagement",
                "metrics.all_conversions_from_store_visit", "metrics.all_conversions_from_store_website", "metrics.all_conversions_value", "metrics.all_conversions_value_by_conversion_date", 
                "metrics.auction_insight_search_absolute_top_impression_percentage", "metrics.auction_insight_search_impression_share", "metrics.auction_insight_search_outranking_share", 
                "metrics.auction_insight_search_overlap_rate", "metrics.auction_insight_search_position_above_rate", "metrics.auction_insight_search_top_impression_percentage",
                "metrics.average_cost", "metrics.average_cpc", "metrics.average_cpe", "metrics.average_cpm", "metrics.average_cpv", "metrics.average_page_views", "metrics.average_time_on_site", 
                "metrics.bounce_rate", "metrics.clicks", "metrics.content_budget_lost_impression_share", "metrics.content_impression_share", "metrics.content_rank_lost_impression_share",
                "metrics.conversions", "metrics.conversions_by_conversion_date", "metrics.conversions_from_interactions_rate", "metrics.conversions_value", 
                "metrics.conversions_value_by_conversion_date", "metrics.cost_micros", "metrics.cost_per_all_conversions", "metrics.cost_per_conversion",
                "metrics.cost_per_current_model_attributed_conversion", "metrics.cross_device_conversions", "metrics.ctr", "metrics.current_model_attributed_conversions",
                "metrics.current_model_attributed_conversions_from_interactions_rate", "metrics.current_model_attributed_conversions_from_interactions_value_per_interaction",
                "metrics.current_model_attributed_conversions_value", "metrics.current_model_attributed_conversions_value_per_cost", "metrics.eligible_impressions_from_location_asset_store_reach", 
                "metrics.engagement_rate", "metrics.engagements", "metrics.gmail_forwards", "metrics.gmail_saves", "metrics.gmail_secondary_clicks", "metrics.impressions",
                "metrics.interaction_event_types", "metrics.interaction_rate", "metrics.interactions", "metrics.invalid_click_rate", "metrics.invalid_clicks", "metrics.optimization_score_uplift",
                "metrics.optimization_score_url", "metrics.percent_new_visitors", "metrics.phone_calls", "metrics.phone_impressions", "metrics.phone_through_rate",
                "metrics.publisher_organic_clicks", "metrics.publisher_purchased_clicks", "metrics.publisher_unknown_clicks", "metrics.relative_ctr", 
                "metrics.search_absolute_top_impression_share", "metrics.search_budget_lost_absolute_top_impression_share", "metrics.search_budget_lost_impression_share", 
                "metrics.search_budget_lost_top_impression_share", "metrics.search_click_share", "metrics.search_exact_match_impression_share", "metrics.search_impression_share",
                "metrics.search_rank_lost_absolute_top_impression_share", "metrics.search_rank_lost_impression_share", "metrics.search_rank_lost_top_impression_share",
                "metrics.search_top_impression_share", "metrics.sk_ad_network_conversions", "metrics.top_impression_percentage", "metrics.value_per_all_conversions",
                "metrics.value_per_all_conversions_by_conversion_date", "metrics.value_per_conversion", "metrics.value_per_conversions_by_conversion_date", 
                "metrics.value_per_current_model_attributed_conversion", "metrics.video_quartile_p100_rate", "metrics.video_quartile_p25_rate", "metrics.video_quartile_p50_rate", 
                "metrics.video_quartile_p75_rate", "metrics.video_view_rate", "metrics.video_views", "metrics.view_through_conversions", 
                "metrics.view_through_conversions_from_location_asset_click_to_call", "metrics.view_through_conversions_from_location_asset_directions", 
                "metrics.view_through_conversions_from_location_asset_menu", "metrics.view_through_conversions_from_location_asset_order",
                "metrics.view_through_conversions_from_location_asset_other_engagement", "metrics.view_through_conversions_from_location_asset_store_visits",
                "metrics.view_through_conversions_from_location_asset_website", };
            StringBuilder query = new StringBuilder();

            if (tableName=="ad_group")
            {
                query.Append("SELECT ");
                var Properties = obj.GetType().GetProperties();
                List<string> propWithout = new List<string>();

                foreach (var fields in adGroupSelectableList)
                {
                    propWithout.Add(fields.Replace("_", ""));
                    
                }
               
                for (int i = 0; i < Properties.Length; i++)
                {


                    if (propWithout.Contains(Properties[i].Name.ToLower()))
                    {
                        var index = propWithout.IndexOf(Properties[i].Name.ToLower());
                        query.Append(tableName + "." + adGroupSelectableList[index]);
                        if (i < Properties.Length - 1)
                        {
                            query.Append(", ");
                        }
                    }
                }
            }
            Metrics metrics = new Metrics();
            if (tableName=="campaign")
            {

                query.Append("SELECT ");
                var Properties = obj.GetType().GetProperties();
                var metricProperties = metrics.GetType().GetProperties();
                List<string> propWithoutMetric = new List<string>();
                List<string> propWithoutCampaign = new List<string>();

                foreach (var fields in campaignSelectableList)
                {
                    var newFields = fields.Replace("campaign.", "");
                    propWithoutCampaign.Add(newFields.Replace("_", ""));
                }
                foreach (var fields in metricsForCampaign)
                {
                    var newFields = fields.Replace("metrics.", "");
                    propWithoutMetric.Add(newFields.Replace("_", ""));
                }
                for (int i = 0; i < Properties.Length; i++)
                {


                    if (propWithoutCampaign.Contains(Properties[i].Name.ToLower()))
                    {
                        var index = propWithoutCampaign.IndexOf(Properties[i].Name.ToLower());
                        query.Append(campaignSelectableList[index]);
                        if (i < Properties.Length - 1)
                        {
                            query.Append(", ");
                        }
                    }
                }
                for (int i = 0; i < metricProperties.Length; i++)
                {
                    

                    if (propWithoutMetric.Contains(metricProperties[i].Name.ToLower()))
                    {
                        var index = propWithoutMetric.IndexOf(metricProperties[i].Name.ToLower());
                        query.Append(metricsForCampaign[index]);
                        if (i < metricProperties.Length - 1)
                        {
                            query.Append(", ");
                        }
                    }
                }
            }         

           
            if (query[query.Length - 2] == ',') query.Remove(query.Length - 2, 2);
            query.Append(" FROM " + tableName);
            query.Append(" ");
            if (campaignId!=null)
            {
                query.Append("WHERE campaign.id =" + campaignId);
            }
          

           

            return query.ToString();
        }

        public static string BUildQueryDynamic(Type resourceType, string resourceName)
        {
            StringBuilder query = new StringBuilder();
            query.Append("SELECT ");

            // Get all properties of the resource class
            var properties = resourceType.GetProperties();

            // Iterate through the properties and append their field names to the query
            for (int i = 0; i < properties.Length; i++)
            {
                var fieldName = properties[i].GetCustomAttribute<FieldNameAttribute>()?.Name;
                if (fieldName != null)
                {
                    query.Append(resourceName + "." + fieldName);
                    if (i < properties.Length - 1)
                    {
                        query.Append(", ");
                    }
                }
            }

            // Remove the trailing comma if present
            if (query[query.Length - 2] == ',') query.Remove(query.Length - 2, 2);

            query.Append(" FROM " + resourceName);
            return query.ToString();
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FieldNameAttribute : Attribute
    {
        public string Name { get; set; }

        public FieldNameAttribute(string name)
        {
            Name = name;
        }
    }
}
