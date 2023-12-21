# OPC Gateway :factory::arrow_right::desktop_computer:

## *Open* OPC UA Web API Data Gateway



## Description

**OPC Gateway** is a self hosted ASP.NET Core 6 WebAPI application which enables applications to read and write data using a OPC UA Protocol using a simple REST API.



## Configuration

The APi is fully configurable using the files below:

- <mark>appconfig.json</mark> (general settings)

- <mark>OpcUaApi.config</mark> (OPC UA Client settings)

- <mark>OpcNodes.csv</mark> (Tag list configuration)

## API Description

There are 2 APIs planned for **OPC Gateway**:

- Legacy (`/legacyapi`)

- Default (`/api`)

### Legacy API (`legacyapi/`)

A simple REST API that uses a small JSON object to carry data to the client.

_Full description will be added in future_

### Default API (`api/`)

This API has more verbose in JSON objects, so it's easier to access and manipulate. Note that the under the hood code is the same for both APIs. Just the JSON objects and endpoints changes.    

_Full description will be added in the future_

## Logging

*Soon*

## Instalation

*Soon*
