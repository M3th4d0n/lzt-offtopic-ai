namespace offtopic_ai.Models;

class Thread
{
    public int thread_id { get; set; }
    public string thread_title { get; set; }
    public string creator_username { get; set; }
    public long thread_create_date { get; set; }
    public Links links { get; set; }
    public FirstPost first_post { get; set; }
}
class FirstPost
{
    public string post_body_plain_text { get; set; }
    public string post_body_html { get; set; } 
}

class Links
{
    public string permalink { get; set; }
}
