# Lytics Connector for Sitecore

The Lytics Connector for Sitecore enables seamless integration between the Lytics customer data platform and Sitecore, empowering personalized content delivery based on real-time customer insights. This module leverages Sitecore's rules engine to provide dynamic Lytics segment personalization.

## How It Works

The connector integrates with your existing Lytics setup to automatically pull active Lytics segments into Sitecore for personalization.

1. **Track User Activities**: Ensure your site is set up to track user activities with Lytics.
2. **Automatic Segment Synchronization**: Once activated, the connector will continuously sync active Lytics segments with Sitecore.

## Getting Started

**Prerequisites**: Before proceeding, ensure that your Sitecore site is configured with Lytics tracking.

1. **Install the NuGet Package**:
   - Download and install the NuGet package from [NuGet.org](https://www.nuget.org/packages/LyticsSitecore.Connector/1.0.0).
   
2. **Configuration File**:
   - The NuGet package will deliver a `lytics.config` file to `app_config/include/lytics.config`.
   - Modify the `lytics.config` file:
     - Insert your Lytics API key. 

3. **Connector Initialization**:
   - On the first load, the connector will install itself by adding the necessary Sitecore items for personalization. This includes:
     - A node for segments.
     - Rules conditions.
     - Updates to the conditional rendering rules definition to incorporate Lytics rules.

4. **Personalize Your Content**:
   - Use Sitecore's personalization features to customize content based on Lytics segments, just as you would with other rules in the rules set editor.

## Benefits

- **Real-Time Personalization**: Deliver personalized experiences based on real-time customer data.
- **Seamless Integration**: Easy integration with your existing Sitecore and Lytics setup.
- **Enhanced Customer Experience**: Leverage detailed customer insights to drive engagement and conversion.
